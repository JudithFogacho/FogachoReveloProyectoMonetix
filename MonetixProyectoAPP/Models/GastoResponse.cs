using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonetixProyectoAPP.Models;

public record GastoResponse
{
    public int IdGasto { get; init; }
    public DateTime FechaRegristo { get; init; }
    public DateTime FechaFinal { get; init; }
    public int Categorias { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public double Valor { get; init; }
    public double ValorPagado { get; init; }
    public int Estados { get; init; }
    public int IdUsuario { get; init; }
    public UsuarioResponse? Usuario { get; init; }

    public string ColorEstado => Estados switch
    {
        (int)Estado.Finalizado => "#81C784", // Verde
        (int)Estado.Atrasado => "#E57373",   // Rojo
        (int)Estado.Pendiente => "#FFD54F",   // Amarillo
        _ => "#FFFFFF"                        // Blanco por defecto
    };
}

public record UsuarioResponse
{
    public int IdUsuario { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Apellido { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public record CreateGastoRequest(
    int UserId,
    DateTime FechaFinal,
    string Categoria,
    string Descripcion,
    double Valor);

public record UpdateGastoRequest(
    DateTime? FechaFinal = null,
    string? Categoria = null,
    string? Descripcion = null,
    double? Valor = null);

public record PagoRequest(double Valor);

public record ResumenGastos(
    int TotalGastos,
    int GastosPendientes,
    int GastosAtrasados,
    int GastosFinalizados,
    double ValorTotal,
    double ValorPagado,
    double ValorPendiente);
