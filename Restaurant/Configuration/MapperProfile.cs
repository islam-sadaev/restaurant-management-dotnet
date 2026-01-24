using Restaurant.ViewModels;
using Restaurant.ViewModels.tafels;
using System.Globalization;

namespace Restaurant.Configuration
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<KlantTafelReserverenViewModel, Reservatie>();

            CreateMap<Reservatie, ReservatieMetTijdSlotViewModel>()
                          .ForMember(dest => dest.Naam, opt => opt.MapFrom(src => $"{src.Tijdslot.Naam}"));

            CreateMap<Bestelling, BestellingAfrekenenViewModel>()
                .ForMember(dest => dest.Productnaam, opt => opt.MapFrom(src => src.Product.Naam))
                .ForMember(dest => dest.Prijs, opt => opt.MapFrom(src => src.Product.PrijsProducten
                .OrderBy(p => p.ProductId)
                .OrderBy(p => p.DatumVanaf)
                .LastOrDefault()));
            CreateMap<BestellingViewmodel, Bestelling>();
            CreateMap<BestellingItemViewModel, Bestelling>();

            CreateMap<Reservatie, ReservatieMetTijdIdViewModel>().ReverseMap();

            CreateMap<Reservatie, ReservatieEditViewModel>().ReverseMap();

            CreateMap<Reservatie, ReservatieDetailsViewModel>().ReverseMap();

            CreateMap<Reservatie, BevestigingsOverzichtViewModel>().ReverseMap();
            CreateMap<Reservatie, BevestigingsOverzichtMetIDViewModel>().ReverseMap();

            CreateMap<Bestelling, BestellingsMetStatusViewModel>()
                .ForMember(dest => dest.ProductNaam, opt => opt.MapFrom(src => src.Product.Naam))
                .ForMember(dest => dest.Naam, opt => opt.MapFrom(src => src.Reservatie.CustomUser.Voornaam))
                .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.Product.Categorie.TypeId));

            CreateMap<CategorieType, CategorieTypeViewModel>();
            CreateMap<Categorie, CategorieViewModel>();

            CreateMap<Product, ProductViewModel>();
            CreateMap<Reservatie, AfrekenenViewModel>().ForMember(dest => dest.Bestellingen, conf => conf.Ignore()); ;
            CreateMap<Product, ProductPrijsViewModel>();
            CreateMap<Product, DrankenViewModel>().ReverseMap();
            CreateMap<Product, DrankenEditViewModel>().ReverseMap();
            CreateMap<DrankenViewModel, PrijsProduct>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id));
            CreateMap<Product, EtenViewModel>().ReverseMap();

            CreateMap<Product, EtenOverzichtItem>()
            .ForMember(dest => dest.CategorieNaam, opt => opt.MapFrom(src => src.Categorie.Naam));

            CreateMap<Product, EtenViewModel>().ReverseMap();
            CreateMap<Product, EtenEditViewModel>().ReverseMap();
            CreateMap<EtenViewModel, PrijsProduct>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id));

            CreateMap<Tafel, VMTafelEdit>();
            CreateMap<VMTafelAanmaken, Tafel>();

            CreateMap<Categorie, CategorieenViewModel>();
            CreateMap<CategorieenCreateViewModel, Categorie>();
            CreateMap<Categorie, CategorieReassignViewModel>();

            //mapping vr params
            CreateMap<IEnumerable<Parameter>, ParameterViewModel>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    var vm = new ParameterViewModel();
                    var dict = src.ToDictionary(p => p.Naam, p => p.Waarde, StringComparer.OrdinalIgnoreCase);

                    // Loop automatisch door alle properties van de ViewModel
                    foreach (var prop in typeof(ParameterViewModel).GetProperties())
                    {
                        if (dict.TryGetValue(prop.Name, out var stringValue))
                        {
                            // Probeer de string waarde om te zetten naar het juiste type (int, decimal, bool, etc.)
                            object? convertedValue = ConvertValue(stringValue, prop.PropertyType);
                            prop.SetValue(vm, convertedValue);
                        }
                    }
                    return vm;
                });

            //VM -> Lijst
            CreateMap<ParameterViewModel, List<Parameter>>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest == null) dest = new List<Parameter>();

                    // Loop automatisch door alle properties van de ViewModel
                    foreach (var prop in typeof(ParameterViewModel).GetProperties())
                    {
                        // Sla de ID property over, die moten we ni laten zien
                        if (prop.Name == "Id") continue;

                        var value = prop.GetValue(src);
                        string stringValue = "";

                        // formatting voor decimalen en datums
                        if (value is decimal d) stringValue = d.ToString(CultureInfo.InvariantCulture);
                        else if (value is TimeSpan t) stringValue = t.ToString(@"hh\:mm");
                        else if (value != null) stringValue = value.ToString() ?? "";

                        // Update/voegtoe als empty is
                        var item = dest.FirstOrDefault(p => p.Naam == prop.Name);
                        if (item != null) item.Waarde = stringValue;
                        else if (value != null) dest.Add(new Parameter { Naam = prop.Name, Waarde = stringValue });
                    }
                    return dest;
                });
        }

        // --- helper voor type converting ---
        private object? ConvertValue(string? value, Type targetType)
        {
            if (string.IsNullOrEmpty(value)) return null;

            var actualType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (actualType == typeof(string)) return value;
            if (actualType == typeof(int)) return int.TryParse(value, out int i) ? i : null;
            if (actualType == typeof(bool)) return bool.TryParse(value, out bool b) ? b : false;
            if (actualType == typeof(decimal)) return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal d) ? d : null;
            if (actualType == typeof(TimeSpan)) return TimeSpan.TryParse(value, out TimeSpan t) ? t : null;

            return Convert.ChangeType(value, actualType, CultureInfo.InvariantCulture);
        }
    }
}