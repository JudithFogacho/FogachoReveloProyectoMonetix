using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MonetixFogachoReveloAPI.Data.Models;

public partial class Usuario
{
    [Key]
    public int IdUsuario { get; set; }
    [Required]
    public string? Nombre { get; set; }
    [Required]
    public string? Apellido { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }


    public virtual ICollection<Gasto>? Gastos { get; set; }
}
