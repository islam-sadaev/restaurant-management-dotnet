using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Restaurant.Configuration.MailService;
using Restaurant.Controllers;
using Restaurant.Data.UnitOfWork;
using Restaurant.Models;
using Restaurant.ViewModels;
using System.Security.Claims;

namespace RestaurantTest.Controllers
{
    internal class ReservatieControllerTest
    {
        private Mock<IUnitOfWork> _mockuow;
        private ReservatieController _controller;
        private Mock<IMapper> _mockMapper;
        private Mock<IEmailSender> _mockEmailsender;

        private Mock<UserManager<CustomUser>> _mockUserManager;

        [SetUp]
        public void Setup()
        {
            _mockuow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockEmailsender = new Mock<IEmailSender>();

            _mockUserManager = CreateUserManagerMock();

            _controller = new ReservatieController(
                _mockuow.Object,
                _mockMapper.Object,
                _mockEmailsender.Object,
                _mockUserManager.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        private Mock<UserManager<CustomUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<CustomUser>>();

            return new Mock<UserManager<CustomUser>>(
                store.Object,
                null, // IOptions<IdentityOptions>
                null, // IPasswordHasher<CustomUser>
                null, // IEnumerable<IUserValidator<CustomUser>>
                null, // IEnumerable<IPasswordValidator<CustomUser>>
                null, // ILookupNormalizer
                null, // IdentityErrorDescriber
                null, // IServiceProvider
                null  // ILogger<UserManager<CustomUser>>
            );
        }

        [Test]
        public async Task Test0Aantal_returnsMagNiet0Zijn()
        {
            //arragne
            var testReservatie = new KlantTafelReserverenViewModel { Datum = DateTime.Today, AantalPersonen = 0, TijdSlotId = 1 };
            var expectedTijdslot = new List<Tijdslot> {
                new() { Id = 1, Naam = "19 uur", Actief = true },
            };
            var mapperReservatie = new Reservatie { Datum = DateTime.Today, AantalPersonen = 0, TijdSlotId = 1 };
            var User = new ClaimsPrincipal { };
            var expectedUser = new CustomUser { Id = "1" };
            var allReservatie = new List<Reservatie>
            {
                new() { Datum = DateTime.Today, AantalPersonen = 2, TijdSlotId = 1 }
            };
            var allExperctedReservatie = new List<ReservatieMetTijdIdViewModel>
            {
                new() { Datum = DateTime.Today, AantalPersonen = 2, TijdSlotId = 1 }
            };
            var paraTotaal = new Parameter { Waarde = "50" };

            _mockuow.Setup(uow => uow.TijdslotRepository.GetAllAsync()).ReturnsAsync(expectedTijdslot);
            _mockMapper.Setup(m => m.Map<Reservatie>(It.IsAny<KlantTafelReserverenViewModel>())).Returns(mapperReservatie);

            _mockUserManager
                .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(expectedUser);

            _mockuow.Setup(uow => uow.ReservatiesRepository.GetAllReservatieMetTijdSlotAsync()).ReturnsAsync(allReservatie);
            _mockMapper.Setup(m => m.Map<List<ReservatieMetTijdIdViewModel>>(It.IsAny<List<Reservatie>>())).Returns(allExperctedReservatie);

            _mockuow.Setup(uow => uow.ParameterRepository.GetByIdAsync(2)).ReturnsAsync(paraTotaal);

            //Act
            var result = await _controller.TafelReserveren(testReservatie);

            //Assert
            Assert.That(result, Is.TypeOf<ViewResult>());

            Assert.IsFalse(_controller.ModelState.IsValid);

            Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));

            Assert.That(
                _controller.ModelState[string.Empty].Errors[0].ErrorMessage,
                Is.EqualTo("Het moet meer als 0 zijn")
            );
        }

        [Test]
        public async Task TestReservatie_Werkt()
        {
            //arragne
            var testReservatie = new KlantTafelReserverenViewModel { Datum = DateTime.Today, AantalPersonen = 3, TijdSlotId = 1 };
            var expectedTijdslot = new List<Tijdslot> {
                new() { Id = 1, Naam = "19 uur", Actief = true },
            };
            var mapperReservatie = new Reservatie { Datum = DateTime.Today, AantalPersonen = 3, TijdSlotId = 1 };
            var expectedReservatie = new Reservatie { Datum = DateTime.Today, AantalPersonen = 3, TijdSlotId = 1 };
            var User = new ClaimsPrincipal { };
            var expectedUser = new CustomUser { Id = "1" };
            var allReservatie = new List<Reservatie>
            {
                new() { Datum = DateTime.Today, AantalPersonen = 2, TijdSlotId = 1 }
            };
            var allExperctedReservatie = new List<ReservatieMetTijdIdViewModel>
            {
                new() { Datum = DateTime.Today, AantalPersonen = 2, TijdSlotId = 1 }
            };
            var paraTotaal = new Parameter { Waarde = "50" };

            _mockuow.Setup(uow => uow.TijdslotRepository.GetAllAsync()).ReturnsAsync(expectedTijdslot);
            _mockMapper.Setup(m => m.Map<Reservatie>(It.IsAny<KlantTafelReserverenViewModel>())).Returns(mapperReservatie);

            _mockUserManager
                .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(expectedUser);

            _mockuow.Setup(uow => uow.ReservatiesRepository.GetAllReservatieMetTijdSlotAsync()).ReturnsAsync(allReservatie);
            _mockMapper.Setup(m => m.Map<List<ReservatieMetTijdIdViewModel>>(It.IsAny<List<Reservatie>>())).Returns(allExperctedReservatie);

            _mockuow.Setup(uow => uow.ParameterRepository.GetByIdAsync(2)).ReturnsAsync(paraTotaal);
            //Act
            var result = await _controller.TafelReserveren(testReservatie);

            //Assert
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());

            var redirectResult = result as RedirectToActionResult;

            Assert.That(redirectResult.ActionName, Is.EqualTo("BevestigingsOverzicht"));

            Assert.That(redirectResult.RouteValues, Is.Not.Null);

            Assert.That(redirectResult.RouteValues["AantalPersonen"], Is.EqualTo(3));
        }

        [Test]
        public async Task TestPlaatsenOnevenNaarEvenWerkt_GeenWeizingPlek()
        {
            //arragne
            var testReservatie = new KlantTafelReserverenViewModel { Datum = DateTime.Today, AantalPersonen = 5, TijdSlotId = 1 };
            var expectedTijdslot = new List<Tijdslot> {
                new() { Id = 1, Naam = "19 uur", Actief = true },
            };
            var mapperReservatie = new Reservatie { Datum = DateTime.Today, AantalPersonen = 5, TijdSlotId = 1 };
            var expectedReservatie = new Reservatie { Datum = DateTime.Today, AantalPersonen = 5, TijdSlotId = 1 };
            var User = new ClaimsPrincipal { };
            var expectedUser = new CustomUser { Id = "1" };
            var allReservatie = new List<Reservatie>
            {
                new() { Datum = DateTime.Today, AantalPersonen = 45, TijdSlotId = 1 }
            };
            var allExperctedReservatie = new List<ReservatieMetTijdIdViewModel>
            {
                new() { Datum = DateTime.Today, AantalPersonen = 45, TijdSlotId = 1 }
            };
            var paraTotaal = new Parameter { Waarde = "50" };

            _mockuow.Setup(uow => uow.TijdslotRepository.GetAllAsync()).ReturnsAsync(expectedTijdslot);
            _mockMapper.Setup(m => m.Map<Reservatie>(It.IsAny<KlantTafelReserverenViewModel>())).Returns(mapperReservatie);

            _mockUserManager
                .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(expectedUser);

            _mockuow.Setup(uow => uow.ReservatiesRepository.GetAllReservatieMetTijdSlotAsync()).ReturnsAsync(allReservatie);
            _mockMapper.Setup(m => m.Map<List<ReservatieMetTijdIdViewModel>>(It.IsAny<List<Reservatie>>())).Returns(allExperctedReservatie);

            _mockuow.Setup(uow => uow.ParameterRepository.GetByIdAsync(2)).ReturnsAsync(paraTotaal);
            //Act
            var result = await _controller.TafelReserveren(testReservatie);

            //Assert
            Assert.That(result, Is.TypeOf<ViewResult>());

            Assert.IsFalse(_controller.ModelState.IsValid);

            Assert.That(_controller.ModelState.ErrorCount, Is.GreaterThan(0));

            Assert.That(
                _controller.ModelState[string.Empty].Errors[0].ErrorMessage,
                Is.EqualTo("Er is geen plek op deze moment ")
            );
        }
    }
}