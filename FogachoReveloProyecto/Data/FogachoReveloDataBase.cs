using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FogachoReveloProyecto.Models;

    public class FogachoReveloDataBase : DbContext
    {
        public FogachoReveloDataBase (DbContextOptions<FogachoReveloDataBase> options)
            : base(options)
        {
        }

        public DbSet<FogachoReveloProyecto.Models.Gasto> Gasto { get; set; } = default!;

public DbSet<FogachoReveloProyecto.Models.Usuario> Usuario { get; set; } = default!;
    }
