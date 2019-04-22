﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using REYMAN.Models;
using Microsoft.AspNetCore.Authorization;
using BizLogic.Administration;
using Microsoft.AspNetCore.Identity;
using BizData.Entities;
using ServiceLayer.AdminServices;
using BizDbAccess.GenericInterfaces;
using BizDbAccess.Utils;
using ServiceLayer.InvestorServices;
using BizLogic.Planning;
using ServiceLayer.AccountServices;
using BizLogic.Authentication;
using System.Security.Claims;

namespace REYMAN.Controllers
{
    /// <summary>
    /// Manage all the views related to admin powers and actions.
    /// </summary>
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AdminController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IUnitOfWork _context;
        private readonly GetterUtils _getterUtils;

        /// <summary>
        /// Constructor for the controller.
        /// </summary>
        /// <param name="userManager">Object of ASP.NET CORE Identity. Is the repository for Usuario entity</param>
        /// <param name="context">Unit of Work in charge of the access to the database. Configured
        /// in Startup/ConfigureServices</param>
        /// <param name="getterUtils">See description for this interface.</param>
        public AdminController(UserManager<Usuario> userManager,
            IUnitOfWork context,
            IGetterUtils getterUtils,
            SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _context = context;
            _getterUtils = (GetterUtils)getterUtils;
            _signInManager = signInManager;
        }

        /// <summary>
        /// GET method of FirstPage(sort of a home page)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> FirstPage()
        {
            AdminService _adminService = new AdminService(_context, _userManager, _getterUtils);
            FirstPageViewModel vm = new FirstPageViewModel();
            var t = await _adminService.FillNotificationsAsync();
            vm.UserPendings = t.UserPendings;
            vm.Provincias = t.Provincias;
            vm.UO = t.Provincias;

            return View(vm);
        }

        /// <summary>
        /// GET method of  EditPlanes view.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult EditPlanes()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            return View(getter.GetAll("Plan"));
        }

        /// <summary>
        /// POST method of EditPlanes view.
        /// </summary>
        /// <param name="button">Type of the clicked button.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditPlanes(string button)
        {

            var action = button.Split("/");
            GetterAll getter = new GetterAll(_getterUtils, _context);
            var pc = new PlanCommand();
            var user = await _userManager.GetUserAsync(User);
            //pc.InmublesUO = user.UnidadOrganizativa.Inmuebles.Select(x=>x.Direccion);
            pc.InmublesUO = new List<string>();
            if (action[0] == "Add")
                return RedirectToAction("AddPlan", "Admin");
            else
                pc.Set(((IEnumerable<Plan>)getter.GetAll("Plan")).Where(x => x.PlanID.ToString() == action[1]).Single());

            return RedirectToAction("PlanState", "Admin", pc);
        }

        public IActionResult PlanState(PlanCommand cmd)
        {

            return View(cmd);
        }

        /// <summary>
        /// GET method of AddPlan view. This page manage the create Plan entity feature.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult AddPlan()
        {
            return View();
        }

        public IActionResult EditPlan(PlanCommand command)
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            InvestorServices investorServices = new InvestorServices(_context);
            if (command.button == "Edit")
            {
                investorServices.UpdatePlan(command.ToPlan(), (((IEnumerable<Plan>)getter.GetAll("Plan")).Where(x => x.PlanID == command.PlanID).Single()));
                return RedirectToAction("EditPlanes", "Admin");
            }
            else
                return View(command);
        }

        /// <summary>
        /// POST method of AddPlan view.
        /// </summary>
        /// <param name="cmd">Data of new Plan.</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult AddPlan(PlanCommand cmd)
        {
            InvestorServices Is = new InvestorServices(_context);
            //display errors if errors is not null
            Is.RegisterPlan(cmd, out var errors);
            return RedirectToAction("EditPlanes", "Admin");
        }

        /// <summary>
        /// GET method of EditProvincia view. This page manage the edit/update Provincia feature.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult EditProvincia()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            ProvinciaViewModel pvm = new ProvinciaViewModel { GetProvincia = getter.GetAll("Provincia") };
            return View(pvm);
        }

        /// <summary>
        /// POST method of EditProvincia view.
        /// </summary>
        /// <param name="vm">Data of the new edited Provincia</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult EditProvincia(ProvinciaViewModel vm)
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            if (ModelState.IsValid)
            {
                AdminService ad = new AdminService(_context, _userManager, _getterUtils);
                if (vm.button == "Add")
                    ad.RegisterProvincia(vm);
                else
                    ad.DeleteProvincia((getter.GetAll("Provincia") as IEnumerable<Provincia>).Where(x => x.Nombre == vm.Nombre).Single());
                return RedirectToAction("EditProvincia", "Admin");
            }

            ModelState.AddModelError(string.Empty, "An error occured trying to edit the entity Provincia");

            //If we got to here, something went wrong
            vm.GetProvincia = getter.GetAll("Provincia");
            return View(vm);
        }

        [HttpGet]
        public IActionResult EditUOs()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            return View(getter.GetAll("UnidadOrganizativa"));
        }

        public IActionResult PartialSelUO()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            return View(getter.GetAll("UnidadOrganizativa"));
        }

        [HttpPost]
        public IActionResult EditUOs(string button)
        {
            AdminService adminService = new AdminService(_context, _userManager, _getterUtils);
            var action = button.Split("/");
            GetterAll getter = new GetterAll(_getterUtils, _context);
            var pc = new PlanCommand();
            if (action[0] == "Add")
                return RedirectToAction("AddUO", "Admin");
            else if (action[0] == "Delete")
                adminService.DeleteUO((getter.GetAll("UnidadOrganizativa") as IEnumerable<UnidadOrganizativa>)
                    .Where(x => x.UnidadOrganizativaID.ToString() == action[1]).Single());
            return RedirectToAction("EditUOs", "Admin", pc);
        }

        [HttpGet]
        public IActionResult AddUO()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            return View(new UOCommand { Provincias = getter.GetAll("Provincia") as IEnumerable<Provincia> });
        }

        [HttpPost]
        public IActionResult AddUO(UOCommand cmd)
        {
            AdminService adminService = new AdminService(_context, _userManager, _getterUtils);
            //display errors if errors is not null
            adminService.RegisterUO(cmd, out var errors);
            return RedirectToAction("EditUOs", "Admin");
        }

        [HttpGet]
        public IActionResult AddAccionCons()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            return View(new AccionConsCommand { UnidadesMedida = new List<string> { "dollar", "nacional" }, AccionConsts = new List<string> { "karl", "teno" } /*(getter.GetAll("AccionConstructiva") as IEnumerable<Provincia>).Select(x => x.Nombre) }*/});
        }

        [HttpPost]
        public IActionResult AddAccionCons(AccionConsCommand cmd)
        {
            InvestorServices investorServices = new InvestorServices(_context);
            investorServices.RegisterAccionCons(cmd, out var errors);
            return RedirectToAction("FirstPage", "Admin");
        }

        [HttpGet]
        public IActionResult AddInmueble()
        {
            return View(new InmuebleCommand());
        }

        [HttpPost]
        public IActionResult AddInmueble(InmuebleCommand cmd)
        {
            if (ModelState.IsValid)
            {
                InvestorServices investorServices = new InvestorServices(_context);
                investorServices.RegisterInmueble(cmd, "Plaza", out var errors);
                return RedirectToAction("EditPlanes", "Admin");
            }
            return View(cmd);
        }

        [HttpGet]
        public IActionResult AddObjObra()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            var inmuebles= (getter.GetAll("UnidadOrganizativa") as IEnumerable<UnidadOrganizativa>).Where(x=>x.Nombre=="Plaza").Single().Inmuebles.Select(x=>x.Direccion);
            return View(new ObjObraCommand {Inmuebles=inmuebles });
        }
        [HttpPost]
        public IActionResult AddObjObra(ObjObraCommand cmd)
        {
            if (ModelState.IsValid)
            {
                InvestorServices investorServices = new InvestorServices(_context);
                cmd.nombreUO = "Plaza";
                investorServices.RegisterObjObra(cmd, cmd.Direccion, out var errors);
                return RedirectToAction("EditPlanes", "Admin");
            }
            return View(cmd);
        }
        [HttpGet]
        public IActionResult EditObjObras()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context);
            return View(getter.GetAll("ObjetoObra"));
        }
        [HttpPost]
        public IActionResult EditObjObras(string button)
        {
            AdminService adminService = new AdminService(_context, _userManager, _getterUtils);
            var action = button.Split("/");
            InvestorServices investorServices = new InvestorServices(_context);
            GetterAll getter = new GetterAll(_getterUtils, _context);
            var pc = new PlanCommand();
            if (action[0] == "Add")
                return RedirectToAction("AddObjObras", "Admin");
            else if (action[0] == "Delete")
               investorServices.DeleteObjetoObra((getter.GetAll("ObjetoObra") as IEnumerable<ObjetoObra>)
                    .Where(x => x.ObjetoObraID.ToString() == action[1]).Single());
            return RedirectToAction("EditUOs", "Admin", pc);
        }
        public IActionResult Usuarios()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context, _signInManager, _userManager);
            return View(getter.GetAll("Usuario"));
        }

        public async Task<IActionResult> EditUsuario(RegisterUsuarioCommand cmd)
        {
            GetterAll getter = new GetterAll(_getterUtils, _context, _signInManager, _userManager);
            GetterAll getter1 = new GetterAll(_getterUtils, _context);
            if (ModelState.IsValid)
            {
                LoginService loginService = new LoginService(_context, _signInManager, _userManager);
                var us = cmd.ToUsuario();
                us.UnidadOrganizativa = (getter1.GetAll("UnidadOrganizativa") as IEnumerable<UnidadOrganizativa>).Where(x => x.UnidadOrganizativaID == cmd.UO).Single();
                await loginService.EditUserAsync(us, (getter.GetAll("Usuario") as IEnumerable<Usuario>).Where(x => x.Email == cmd.EditEmail).Single());
                return RedirectToAction("Usuarios", "Admin");
            }
            cmd.UOs = getter1.GetAll("UnidadOrganizativa") as IEnumerable<UnidadOrganizativa>;
            return View(cmd);
        }

        [HttpGet]
        public async Task<IActionResult> PendingUsers()
        {
            GetterAll getter = new GetterAll(_getterUtils, _context, _signInManager, _userManager);
            List<Usuario> pending = new List<Usuario>();
            foreach (var item in getter.GetAll("Usuario"))
            {
                if ((await _userManager.GetClaimsAsync(item as Usuario)).Any(c => c.Type == "Pending" && c.Value == "true"))
                    pending.Add(item as Usuario);
            }

            PendingUsersViewModel vm = new PendingUsersViewModel()
            {
                Usuarios = pending,
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> PendingUsers(PendingUsersViewModel vm)
        {
            var user = await _userManager.FindByIdAsync(vm.userID);
            await _userManager.RemoveClaimAsync(user, new Claim("Pending", "true"));
            await _userManager.AddClaimAsync(user, new Claim("Pending", "false"));
            await _userManager.AddClaimAsync(user, new Claim("Permission", "inversionista"));
            _context.Commit();

            GetterAll getter = new GetterAll(_getterUtils, _context, _signInManager, _userManager);
            List<Usuario> pending = new List<Usuario>();
            foreach (var item in getter.GetAll("Usuario"))
            {
                if ((await _userManager.GetClaimsAsync(item as Usuario)).Any(c => c.Type == "Pending" && c.Value == "true"))
                    pending.Add(item as Usuario);
            }
            return RedirectToAction("PendingUsers", "Admin");
        }






        [HttpGet]
        public IActionResult AddMaterial()
        {
            var lvm = new MaterialViewModel();
            lvm.UnidadesMedida = new GetterAll(_getterUtils, _context).GetAll("UnidadMedida") as IEnumerable<UnidadMedida>;
            return View(lvm);
        }

        [HttpPost]
        public IActionResult AddMaterial(MaterialViewModel lvm)
        {
            var unidadMedida = (new GetterAll(_getterUtils, _context).GetAll("UnidadMedida") as IEnumerable<UnidadMedida>).Where(um => um.Nombre == lvm.UnidadMedida).Single();
            var service = new InvestorServices(_context);
            service.RegisterMaterial(new MaterialCommand(null, new Material() { Nombre = lvm.Nombre, UnidadMedida = unidadMedida }), out var errors);
            return RedirectToAction("EditMateriales");
        }


        public IActionResult EditMaterial(MaterialViewModel lvm)
        {
            lvm.UnidadesMedida = new GetterAll(_getterUtils, _context).GetAll("UnidadMedida") as IEnumerable<UnidadMedida>;
            if (ModelState.IsValid && lvm.Button == "post")
            {
                new InvestorServices(_context).UpdateMaterial(new Material() { Nombre = lvm.Nombre, UnidadMedida = (new GetterAll(_getterUtils, _context).GetAll("UnidadMedida") as IEnumerable<UnidadMedida>).Where(um => um.Nombre == lvm.UnidadMedida).Single() },
                                                              new AccionC_Material() { Material = (new GetterAll(_getterUtils, _context).GetAll("Material") as IEnumerable<Material>).Where(mat => mat.MaterialID == lvm.MaterialId).Single() },
                                                              out var errors);

                return RedirectToAction("EditMateriales");
            }
            return View(lvm);
        }

        [HttpGet]
        public IActionResult EditMateriales()
        {
            var materiales = new GetterAll(_getterUtils, _context).GetAll("Material") as IEnumerable<Material>;
            return View(materiales);
        }

        [HttpPost]
        public IActionResult EditMateriales(string button)
        {
            var action = button.Split('/');
            if (action[0] == "Add")
                return RedirectToAction("AddMaterial");
            else if (action[0] == "Delete")
            {
                new InvestorServices(_context).DeleteMaterial((new GetterAll(_getterUtils, _context).GetAll("Material") as IEnumerable<Material>).Where(mat => mat.MaterialID == int.Parse(action[1])).Single());
                return RedirectToAction("EditMateriales");
            }
            else
            {
                var material = (new GetterAll(_getterUtils, _context).GetAll("Material") as IEnumerable<Material>).Where(mat => mat.MaterialID == int.Parse(action[1])).Single();
                return RedirectToAction("EditMaterial", new MaterialViewModel()
                                                        {
                                                            Nombre = material.Nombre,
                                                            MaterialId = material.MaterialID,
                                                            UnidadMedida = material.UnidadMedida.Nombre
                                                        });
            }
        }






        [HttpGet]
        public IActionResult AddUnidadMedida()
        {
            return View(new UnidadMedidaCommand());
        }

        [HttpPost]
        public IActionResult AddUnidadMedida(UnidadMedidaCommand cmd)
        {
            if (ModelState.IsValid)
                new InvestorServices(_context).RegisterUnidadMedida(cmd, out var errors);
            return RedirectToAction("EditUnidadesMedida");
        }

        [HttpGet]
        public IActionResult EditUnidadesMedida()
        {
            var ums = new GetterAll(_getterUtils, _context).GetAll("UnidadMedida") as IEnumerable<UnidadMedida>;
            return View(ums);
        }

        [HttpPost]
        public IActionResult EditUnidadesMedida(string button)
        {
            var action = button.Split('/');
            if (action[0] == "Add")
                return RedirectToAction("AddUnidadMedida");
            else
            {
                new InvestorServices(_context).DeleteUnidadMedida((new GetterAll(_getterUtils, _context).GetAll("UnidadMedida") as IEnumerable<UnidadMedida>).Where(um => um.UnidadMedidaID == int.Parse(action[1])).Single());
                return RedirectToAction("EditUnidadesMedida");
            }
        }






        [HttpGet]
        public IActionResult AddEspecialidad()
        {
            return View(new EspecialidadCommand());
        }

        [HttpPost]
        public IActionResult AddEspecialidad(EspecialidadCommand cmd)
        {
            if (ModelState.IsValid)
                new InvestorServices(_context).RegisterEspecialidad(cmd, out var errors);
            return RedirectToAction("EditEspecialidades");
        }

        [HttpGet]
        public IActionResult EditEspecialidades()
        {
            var ums = new GetterAll(_getterUtils, _context).GetAll("Especialidad") as IEnumerable<Especialidad>;
            return View(ums);
        }

        [HttpPost]
        public IActionResult EditEspecialidades(string button)
        {
            var action = button.Split('/');
            if (action[0] == "Add")
                return RedirectToAction("AddEspecialidad");
            else
            {
                new InvestorServices(_context).DeleteEspecialidad((new GetterAll(_getterUtils, _context).GetAll("Especialidad") as IEnumerable<Especialidad>).Where(esp => esp.EspecialidadID == int.Parse(action[1])).Single());
                return RedirectToAction("EditEspecialidades");
            }
        }
    }
}
