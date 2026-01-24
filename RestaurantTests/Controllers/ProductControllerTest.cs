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
    internal class ProductControllerTest
    {
        private Mock<IUnitOfWork> _mockuow;
        private ProductController _controller;
        private Mock<IMapper> _mockMapper;

        [SetUp]
        public void Setup()
        {
            _mockuow = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();

            _controller = new ProductController(
                _mockuow.Object,
                _mockMapper.Object

            );
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task CreateDrank_ValidModel_RedirectsToDrankOverzicht()
        {
            // Arrange
            var viewModel = new DrankenViewModel
            {
                Naam = "Cola",
                prijs = 2.50m,
            };

            var product = new Product { Id = 1 };
            var prijs = new PrijsProduct();

            _mockMapper
                .Setup(m => m.Map<Product>(viewModel))
                .Returns(product);

            _mockMapper
                .Setup(m => m.Map<PrijsProduct>(viewModel))
                .Returns(prijs);

            _mockuow
                .Setup(c => c.ProductRepository.AddAsync(product))
                .Returns(Task.CompletedTask);

            _mockuow
                .Setup(c => c.PrijsProductRepository.AddAsync(prijs))
                .Returns(Task.CompletedTask);

            _mockuow
                .Setup(c => c.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateDrank(viewModel);

            // Assert
            var redirect = result as RedirectToActionResult;

            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("DrankOverzicht"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Product"));

            Assert.That(prijs.ProductId, Is.EqualTo(product.Id));
            Assert.That(prijs.DatumVanaf, Is.EqualTo(DateTime.Today));
        }

        [Test]
        public async Task CreateEten_ValidModel_RedirectsToEtenOverzicht()
        {
            // Arrange
            var viewModel = new EtenViewModel
            {
                Naam = "Spaghetti",
                prijs = 12.50m,
            };

            var product = new Product { Id = 10 };
            var prijs = new PrijsProduct();

            _mockMapper
                .Setup(m => m.Map<Product>(viewModel))
                .Returns(product);

            _mockMapper
                .Setup(m => m.Map<PrijsProduct>(viewModel))
                .Returns(prijs);

            _mockuow
                .Setup(c => c.ProductRepository.AddAsync(product))
                .Returns(Task.CompletedTask);

            _mockuow
                .Setup(c => c.PrijsProductRepository.AddAsync(prijs))
                .Returns(Task.CompletedTask);

            _mockuow
                .Setup(c => c.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateEten(viewModel);

            // Assert
            var redirect = result as RedirectToActionResult;

            Assert.That(redirect, Is.Not.Null);
            Assert.That(redirect!.ActionName, Is.EqualTo("EtenOverzicht"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Product"));

            Assert.That(prijs.ProductId, Is.EqualTo(product.Id));
            Assert.That(prijs.DatumVanaf, Is.EqualTo(DateTime.Today));
        }
    }
}