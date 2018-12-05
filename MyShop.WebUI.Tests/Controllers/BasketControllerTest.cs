﻿using System;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using MyShop.Services;
using MyShop.WebUI.Controllers;
using MyShop.WebUI.Tests.Mocks;

namespace MyShop.WebUI.Tests.Controllers
{
    [TestClass]
    public class BasketControllerTest
    {
        [TestMethod]
        public void CanAddBasketItem()
        {
            //Setup Test
            IRepository<Basket> baskets = new MockContext<Basket>();
            IRepository<Product> products = new MockContext<Product>();
            IRepository<Order> orders = new MockContext<Order>();
            IRepository<Customer> customers = new MockContext<Customer>();

            var httpContext = new MockHttpContext();

            IBasketService basketService = new BasketService(products, baskets);
            IOrderService orderService = new OrderService(orders);
            var controller = new WebUI.Controllers.BasketController(basketService, orderService, customers);

            //WebUI.Controllers.BasketController controller = new WebUI.Controllers.BasketController(basketService);
            controller.ControllerContext = new System.Web.Mvc.ControllerContext(httpContext, new System.Web.Routing.RouteData(), controller);


            //Act Tests service side
            //basketService.AddToBasket(httpContext, "1");

            //Act Tests controller side
            controller.AddToBasket("1");

            Basket basket = baskets.Collection().FirstOrDefault();


            //Assert Test
            Assert.IsNotNull(basket);
            Assert.AreEqual(1, basket.BasketItems.Count);
            Assert.AreEqual("1", basket.BasketItems.ToList().FirstOrDefault().ProductId);
        }

        [TestMethod]
        public void CanGetSummaryViewModel()
        {
            //Setup Test
            IRepository<Basket> baskets = new MockContext<Basket>();
            IRepository<Product> products = new MockContext<Product>();
            IRepository<Order> orders = new MockContext<Order>();
            IRepository<Customer> customers = new MockContext<Customer>();


            products.Insert(new Product()
            {
                Id = "1",
                Price = 10
            });

            products.Insert(new Product()
            {
                Id = "2",
                Price = 30
            });

            Basket basket = new Basket();
            basket.BasketItems.Add(new BasketItem() { ProductId = "1", Quantity= 2 });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 1 });
            baskets.Insert(basket);


            IBasketService basketService = new BasketService(products, baskets);
            IOrderService orderService = new OrderService(orders);
            var controller = new BasketController(basketService, orderService, customers);

            var httpContext = new MockHttpContext();
            httpContext.Request.Cookies.Add(
                new System.Web.HttpCookie("MyShopBasket") { Value = basket.Id });

            controller.ControllerContext = new System.Web.Mvc.ControllerContext(httpContext
                , new System.Web.Routing.RouteData(), controller);

            var result = controller.BasketSummary() as PartialViewResult;
            var basketSummary = (BasketSummaryViewModel)result.ViewData.Model;

            Assert.AreEqual(3, basketSummary.BasketCount);
            Assert.AreEqual(50, basketSummary.BasketTotal);

        }

        [TestMethod]
        public void CanCheckoutAndCreateOrder()
        {
            IRepository<Product> products = new MockContext<Product>();
            IRepository<Basket> baskets = new MockContext<Basket>();
            IRepository<Order> orders = new MockContext<Order>();
            IRepository<Customer> customers = new MockContext<Customer>();


            products.Insert(new Product(){Id = "1", Price = 10 });
            products.Insert(new Product(){Id = "2", Price = 30 });

            
            Basket basket = new Basket();
            basket.BasketItems.Add(new BasketItem() { ProductId = "1", Quantity = 2, BasketId = basket.Id });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 1, BasketId = basket.Id });

            baskets.Insert(basket);

            IBasketService basketService = new BasketService(products, baskets);
            IOrderService orderService = new OrderService(orders);

            customers.Insert(new Customer() { Id = "1", Email = "test@le.com", ZipCode = "1234" });
            IPrincipal FakeUser = new GenericPrincipal(
                new GenericIdentity("test@le.com", "Forms"), null);


            var controller = new BasketController(basketService, orderService, customers);

            var httpContext = new MockHttpContext();

            httpContext.User = FakeUser;
            httpContext.Request.Cookies.Add(
                new System.Web.HttpCookie("MyShopBasket") { Value = basket.Id });

            controller.ControllerContext = new System.Web.Mvc.ControllerContext(httpContext
                , new System.Web.Routing.RouteData(), controller);

            //Act
            Order order = new Order();
            controller.Checkout(order);

            //Assert
            Assert.AreEqual(2, order.OrderItems.Count);
            Assert.AreEqual(0, basket.BasketItems.Count);

            Order orderInRep = orders.Find(order.Id);
            Assert.AreEqual(2, orderInRep.OrderItems.Count);





        }
    }
}
