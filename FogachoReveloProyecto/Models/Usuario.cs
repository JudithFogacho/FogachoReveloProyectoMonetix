using System.ComponentModel.DataAnnotations;
namespace FogachoReveloProyecto.Models
{
    public class Usuario
    {
        //Creación de los atributos del usuario
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

        // Propiedad de navegación para la relación uno a muchos
        public virtual ICollection<Gasto>? Gastos { get; set; }
    }
}