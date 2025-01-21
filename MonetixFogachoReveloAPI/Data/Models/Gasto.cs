using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonetixFogachoReveloAPI.Data.Models;

public class Gasto
{
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


    [Required]
    public int IdUsuario { get; set; }


    [ForeignKey("IdUsuario")]
    public virtual Usuario? Usuario { get; set; }



    public double CalcularValorGasto(double valorPago)
    {
        if (Valor > 0 && valorPago > 0)
        {
            ValorPagado += valorPago;
            double nuevoValor = (double)(Valor - ValorPagado);

            Valor = nuevoValor < 0 ? 0 : nuevoValor;
            //usamos el metodo de recursividad
            ActualizacionPagos();
            return (double)Valor;
        }
        return 0;
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