﻿using Cozy.Domain.Models;
using Cozy.Service.Interface;
using Cozy.WebUI.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Cozy.WebUI.Controllers
{
    [Authorize(Roles ="Tenant")]
    public class TenantController : Controller 
    {
        private ITenantServices _tenantServices;
        private ILeaseServices _leaseServices;
        private IPropertyServices _propertyServices;
        private IBankServices _bankServices;
        private IPaymentServices _paymentServices;

        public TenantController(ITenantServices tenantServices, 
            ILeaseServices leaseServices, 
            IPropertyServices propertyServices,
            IBankServices bankServices,
            IPaymentServices paymentServices)
        {
            _tenantServices = tenantServices;
            _leaseServices = leaseServices;
            _propertyServices = propertyServices;
            _bankServices = bankServices;
            _paymentServices = paymentServices;
        }

        private string GetUserId()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var claims = identity.Claims.ToList();

            return claims[0].Value;
        }

        public IActionResult Index()
        {
            // tenant - TenantService
            var t = _tenantServices.GetTenantById(GetUserId());

            // property
            var p = _propertyServices.GetPropertyByCurrentTenant(GetUserId());

            // lease - active
            var l = _leaseServices.GetLeaseByPropertyIdAndTenantId(p.Id, t.Id);

            var model = new TenantPropertyViewModel()
            {
                Property = p,
                Tenant = t
            };

            return View(model);
        }

        public IActionResult Bank(string id)
        {
            var tenant = _tenantServices.GetTenantById(id);
            List<Bank> banks = _bankServices.GetBanksByTenantId(id);

            var viewModel = new TenantBanksViewModel();
            viewModel.Banks = banks;
            viewModel.Tenant = tenant;

            return View(viewModel);
        }

        public IActionResult Payments()
        {
            
            var p = _propertyServices.GetPropertyByCurrentTenant(GetUserId());
            var payments = _paymentServices.GetPaymentsByPropertyId(p.Id);
            payments = payments.Take(3).ToList();

            return View(payments);
        }

        public IActionResult CreateBank(Bank newBank)
        {
            // save it
            _bankServices.CreateBank(newBank);

            // redirect to a page
            return RedirectToAction("Bank", new { id = newBank.UserId });
        }
    }
}
