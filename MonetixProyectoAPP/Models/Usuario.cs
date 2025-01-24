using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonetixProyectoAPP.Models
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

        // Propiedad de navegación para la relación uno a muchos con Gastos
        public virtual ICollection<Gasto>? Gastos { get; set; }

        // Constructor para inicializar la colección de Gastos
        public Usuario()
        {
            Gastos = new List<Gasto>();
        }
    }
}