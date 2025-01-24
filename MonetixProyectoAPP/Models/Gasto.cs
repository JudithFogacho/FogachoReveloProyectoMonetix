using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonetixProyectoAPP.Models
{
    public class Gasto
    {
        //Atributos
        [Key]
        public int IdGasto { get; set; }
        [Required]
        public DateTime FechaRegristo { get; set; }
        [Required]
        public DateTime FechaFinal { get; set; }

        [Required]
        public Categoria? Categorias { get; set; }
        [Required]
        public string? Descripcion { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public double? Valor { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public double ValorPagado { get; set; }
        public Estado Estados { get; set; }
        public string ColorEstado { get; private set; }

        // Nuevas propiedades para la relación con Usuario
        [Required]
        public int IdUsuario { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuario? Usuario { get; set; }

        // Los métodos existentes se mantienen igual
        public void AsignarColorEstado()
        {
            switch (Estados)
            {
                case Estado.Atrasado:
                    ColorEstado = "#E57373"; // Rojo
                    break;
                case Estado.Pendiente:
                    ColorEstado = "#FFD54F"; // Amarillo
                    break;
                case Estado.Finalizado:
                    ColorEstado = "#81C784"; // Verde
                    break;
                default:
                    ColorEstado = "#FFFFFF"; // Blanco por defecto
                    break;
            }
        }

        public double CalcularValorGasto(double valorPago)
        {
            if (Valor.HasValue && valorPago > 0)
            {
                ValorPagado += valorPago;
                double nuevoValor = (Valor.Value - ValorPagado);
                // Aseguramos que el valor no sea negativo
                Valor = nuevoValor < 0 ? 0 : nuevoValor;
                ActualizacionPagos();
                return Valor.Value;
            }
            return Valor ?? 0;
        }

        public void ActualizacionPagos()
        {
            DateTime fechaActual = DateTime.Today;
            DateTime fechaFinalSinHora = FechaFinal.Date;

            if (Valor != null && ValorPagado == Valor)
            {
                Estados = Estado.Finalizado;
            }
            else if (fechaActual > fechaFinalSinHora && Valor > 0)
            {
                Estados = Estado.Atrasado;
            }
            else if (fechaActual <= fechaFinalSinHora && Valor > 0)
            {
                Estados = Estado.Pendiente;
            }
        }

        public void ValidarValor()
        {
            ActualizacionPagos();
        }
    }
}