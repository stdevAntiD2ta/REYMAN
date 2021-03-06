﻿using BizData.Entities;
using BizDbAccess.GenericInterfaces;
using DataLayer.EfCode;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace BizDbAccess.User
{
    /// <summary>
    /// Specific implementation of a repository for EntityFrameworkCore.
    /// </summary>
    public class PlanDbAccess : IEntityDbAccess<Plan>
    {
        public readonly EfCoreContext _context;

        public PlanDbAccess(IUnitOfWork context)
        {
            _context = (EfCoreContext)context;
        }

        public void Add(Plan entity)
        {
            _context.Planes.Add(entity);
        }

        public void Delete(Plan entity)
        {
            if (_context.Planes.Find(entity.PlanID) != null)
            {
                _context.Planes.Remove(entity);
            }
        }

        public IEnumerable<Plan> GetAll()
        {
            return _context.Planes;
        }

        public Plan Update(Plan entity, Plan plan)
        {
            
            if (plan == null)
                throw new Exception("No existe plan que se quiere actualizar");

            if (entity.AccionesConstructivas == null)
                entity.AccionesConstructivas = new List<AccionConstructiva>();

            plan.Año = entity.Año == 0 ? plan.Año : entity.Año;
            plan.Presupuesto = entity.Presupuesto == 0 ? plan.Presupuesto : entity.Presupuesto;
            plan.TipoPlan = entity.TipoPlan ?? plan.TipoPlan;
            plan.AccionesConstructivas = plan.AccionesConstructivas == null ? entity.AccionesConstructivas : (plan.AccionesConstructivas.Concat(entity.AccionesConstructivas)).ToList();

            _context.Planes.Update(plan);
            return plan;
        }

        public Plan GetPlan(int año, string tipo, UnidadOrganizativa uo)
        {
            return _context.Planes.Where(p => p.Año == año && p.TipoPlan == tipo && p.UnidadOrganizativa.Nombre == uo.Nombre).SingleOrDefault();
        }

        public Plan GetPlan(int id)
        {
            return _context.Planes.Find(id);
        }
    }
}
