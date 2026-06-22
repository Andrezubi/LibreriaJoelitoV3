namespace MicroServicioReportes.Dominio.Prototipos;

public interface IPrototipo<out T>
{
    T Clonar();
}
