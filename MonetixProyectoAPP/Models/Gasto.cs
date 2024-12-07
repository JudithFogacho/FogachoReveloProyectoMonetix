using System.ComponentModel.DataAnnotations;

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
        public Categoria Categorias { get; set; }
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



        //Este metodo se utiliza para calcular el valor del gasto
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
        //Este metodo se utiliza para cambiar el estado de los gastos
        public void ActualizacionPagos()
        {
            //Tomamos fechas
            DateTime fechaActual = DateTime.Today;  
            DateTime fechaFinalSinHora = FechaFinal.Date;

            // Si ya está todo pagado el estado será finalizado
            if (Valor != null && ValorPagado == Valor) 
            {
                Estados = Estado.Finalizado;
            }
            //si la fecha se ha pasado de la fecha final y no se ha pagado será Atrasado
            else if (fechaActual > fechaFinalSinHora && Valor > 0)
            {
                Estados = Estado.Atrasado;
            }
            //si la fecha aun no ha pasado la fecha final y no esta pagado será Pendiente
            else if (fechaActual <= fechaFinalSinHora && Valor > 0) 
            {
                Estados = Estado.Pendiente;
            }
        }
        //Este metodo se utiliza para validar el estado finalizado
        public void ValidarValor() 
        {
            ActualizacionPagos();
        }
    }
}
