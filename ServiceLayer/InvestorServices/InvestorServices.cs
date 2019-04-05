﻿using BizData.Entities;
using BizDbAccess.Authentication;
using BizDbAccess.GenericInterfaces;
using BizDbAccess.User;
using BizLogic.Planning;
using BizLogic.Planning.Concrete;
using ServiceLayer.BizRunners;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BizDbAccess.Repositories;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;

namespace ServiceLayer.InvestorServices
{
    public class InvestorServices
    {
        private readonly RunnerWriteDb<PlanCommand, Plan> _runnerPlan;
        private readonly RunnerWriteDb<InmuebleCommand, Inmueble> _runnerInmueble;
        private readonly RunnerWriteDb<ObjObraCommand, ObjetoObra> _runnerObjObra;
        private readonly RunnerWriteDb<AccionConsCommand, AccionConstructiva> _runnerAccionCons;

        private readonly AccionConstructivaDbAccess _accionConstructivaDbAccess;
        private readonly EspecialidadDbAccess _especialidadDbAccess;
        private readonly PlanDbAccess _planDbAccess;
        private readonly InmuebleDbAccess _inmuebleDbAccess;
        private readonly ObjetoObraDbAccess _objetoObraDbAccess;
        private readonly UnidadOrganizativaDbAccess _unidadOrganizativaDbAccess;
        private readonly IUnitOfWork _context;

        public InvestorServices(IUnitOfWork context)
        {
            _context = context;
            _runnerPlan = new RunnerWriteDb<PlanCommand, Plan>(
                new RegisterPlanAction(new PlanDbAccess(_context)), _context);
            _runnerInmueble = new RunnerWriteDb<InmuebleCommand, Inmueble>(
                new RegisterInmuebleAction(new InmuebleDbAccess(_context)), _context);
            _runnerAccionCons = new RunnerWriteDb<AccionConsCommand, AccionConstructiva>(
                new RegisterAccionConsAction(new AccionConstructivaDbAccess(_context)), _context);
            _runnerObjObra = new RunnerWriteDb<ObjObraCommand, ObjetoObra>(
                new RegisterObjObraAction(new ObjetoObraDbAccess(_context)), _context);

            _planDbAccess = new PlanDbAccess(_context);
            _inmuebleDbAccess = new InmuebleDbAccess(_context);
            _objetoObraDbAccess = new ObjetoObraDbAccess(_context);
            _unidadOrganizativaDbAccess = new UnidadOrganizativaDbAccess(_context);
            _accionConstructivaDbAccess = new AccionConstructivaDbAccess(_context);
            _especialidadDbAccess = new EspecialidadDbAccess(_context);
        }

        public long RegisterPlan(PlanCommand cmd, out IImmutableList<ValidationResult> errors)
        {
            var plan = _runnerPlan.RunAction(cmd);

            if (_runnerPlan.HasErrors)
            {
                errors = _runnerPlan.Errors;
                return -1;
            }

            errors = null;
            return plan.PlanID;
        }

        public Plan GetPlan(int año, string tipo)
        {
            return _planDbAccess.GetPlan(año, tipo);
        }

        public Plan UpdatePlan(Plan entity, Plan toUpd)
        {
            var plan = _planDbAccess.Update(entity, toUpd);
            _context.Commit();
            return plan;
        }

        public long RegisterInmueble(InmuebleCommand cmd, string nombreUO, out IImmutableList<ValidationResult> errors)
        {
            var uo = _unidadOrganizativaDbAccess.GetUO(nombreUO);
            cmd.UO = uo;

            var inm = _runnerInmueble.RunAction(cmd);

            if (_runnerInmueble.HasErrors)
            {
                errors = _runnerInmueble.Errors;
                return -1;
            }

            errors = null;
            return inm.InmuebleID;
        }

        public Inmueble UpdateInmueble(Inmueble entity, Inmueble toUpd)
        {
            if (entity.Direccion != null && entity.Direccion != toUpd.Direccion &&
                _inmuebleDbAccess.GetInmueble(entity.UO, entity.Direccion) != null)
            {
                throw new Exception($"Ya existe un Inmueble con direccion {entity.Direccion}");
            }

            var inmueble = _inmuebleDbAccess.Update(entity, toUpd);
            _context.Commit();
            return inmueble;
        }

        public bool CheckForInmuebles(string nombreUO)
        {
            var uo = _unidadOrganizativaDbAccess.GetUO(nombreUO);
            return uo.Inmuebles.Any();
        }

        public Inmueble AddObjObraToInm(Inmueble entity, IEnumerable<ObjetoObra> objsObra)
        {
            if (entity.ObjetosDeObra == null)
            {
                entity.ObjetosDeObra = new List<ObjetoObra>();
            }

            _inmuebleDbAccess.AddObjObra(ref entity, objsObra);
            _context.Commit();
            return entity;
        }

        public long RegisterObjObra(ObjObraCommand vm, string dirInmueble, out IImmutableList<ValidationResult> errors)
        {
            vm.Inmueble = _inmuebleDbAccess.GetInmueble(_unidadOrganizativaDbAccess.GetUO(vm.nombreUO),
                            dirInmueble);

            var objObra = _runnerObjObra.RunAction(vm);

            if (_runnerObjObra.HasErrors)
            {
                errors = _runnerObjObra.Errors;
                return -1;
            }

            errors = null;
            return objObra.ObjetoObraID;
        }

        public ObjetoObra UpdateObjetoObra(ObjetoObra entity, ObjetoObra toUpd)
        {
            if (entity.Nombre != null && entity.Nombre != toUpd.Nombre &&
                _objetoObraDbAccess.GetObjObra(entity.Nombre, entity.Inmueble.Direccion,
                entity.Inmueble.UO.Nombre) != null)
            {
                throw new Exception($"Ya existe un Objeto de Obra con nombre {entity.Nombre}");
            }

            var objObra = _objetoObraDbAccess.Update(entity, toUpd);
            _context.Commit();
            return objObra;
        }

        public long RegisterAccionCons(AccionConsCommand cmd, out IImmutableList<ValidationResult> errors)
        {
            cmd.Plan = _planDbAccess.GetPlan(cmd.PlanID);
            cmd.Especialidad = _especialidadDbAccess.GetEspecialidad(cmd.TipoEspecialidad);
            cmd.ObjetoObra = _objetoObraDbAccess.GetObjObra(cmd.ObjetoObraID);

            var ac = _runnerAccionCons.RunAction(cmd);

            if (_runnerAccionCons.HasErrors)
            {
                errors = _runnerAccionCons.Errors;
                return -1;
            }

            errors = null;
            return ac.AccionConstructivaID;
        }
    }
}