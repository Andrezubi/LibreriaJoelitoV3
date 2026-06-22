using MicroServicioProductos.Aplicacion.DTOs;
using MicroServicioProductos.Dominio.Modelos;
using MicroServicioProductos.Infraestructura.Mensajeria.Rabbit;
using MicroServicioProductos.Infraestructura.Persistencia.FactoriaProductos;
using System.Text.Json;

namespace MicroServicioProductos.Aplicacion.Servicios
{
    public class ProductoSagaServicio
    {


        private readonly ProductoRepositorio _productoRepositorio;

        private readonly PresentacionProductoRepositorio _presentacionRepositorio;

        private readonly ProcessedMessageRepositorio _processedRepositorio;

        private readonly OutboxMessageRepositorio _outboxRepositorio;

        private readonly RabbitMqOptions _options;



        public ProductoSagaServicio(

            ProductoRepositorio productoRepositorio,

            PresentacionProductoRepositorio presentacionRepositorio,

            ProcessedMessageRepositorio processedRepositorio,

            OutboxMessageRepositorio outboxRepositorio,

            RabbitMqOptions options

            )
        {

            _productoRepositorio = productoRepositorio;

            _presentacionRepositorio = presentacionRepositorio;

            _processedRepositorio = processedRepositorio;

            _outboxRepositorio = outboxRepositorio;

            _options = options;

        }





























        private void GuardarMensajeProcesado(

        dynamic mensaje, string routingKey = ""

        )
        {


            _processedRepositorio.Insertar(


                new ProcessedMessage(

                    mensaje.MessageId,

                    mensaje.CorrelationId,

                    routingKey

                    )


                );


        }







        private void CrearEventoRechazado(

            ReservarStockMessageDto mensaje,

            string motivo

            )

        {

            var dto = new StockRechazadoMessageDto
            {

                MessageId =
                    Guid.NewGuid().ToString(),

                CorrelationId =
                    mensaje.CorrelationId,

                IdVenta =
                    mensaje.IdVenta,

                Motivo =
                    motivo

            };



            string payload =

                JsonSerializer.Serialize(

                    dto

                    );




            var outbox = new OutboxMessage(

                dto.MessageId,

                dto.CorrelationId,

                _options.ExchangeName,

                _options.RoutingKeys.StockRechazado,

                nameof(

                    StockRechazadoMessageDto

                    ),

                payload

                );



            _outboxRepositorio.Insertar(

                outbox

                );

        }

        private void CrearEventoLiberado(

            LiberarStockMessageDto mensaje

            )

        {

            var dto = new StockLiberadoMessageDto
            {

                MessageId =
                    Guid.NewGuid().ToString(),

                CorrelationId =
                    mensaje.CorrelationId,

                IdVenta =
                    mensaje.IdVenta

            };



            string payload =

                JsonSerializer.Serialize(

                    dto

                    );



            var outbox = new OutboxMessage(

                dto.MessageId,

                dto.CorrelationId,

                _options.ExchangeName,

                _options.RoutingKeys.StockLiberado,

                nameof(

                    StockLiberadoMessageDto

                    ),

                payload

                );



            _outboxRepositorio.Insertar(

                outbox

                );

        }



        private void CrearEventoReservado(
            ReservarStockMessageDto mensaje)
        {

            var dto = new StockReservadoMessageDto
            {
                MessageId = Guid.NewGuid().ToString(),

                CorrelationId = mensaje.CorrelationId,

                IdVenta = mensaje.IdVenta
            };


            string payload =
                JsonSerializer.Serialize(dto);



            var outbox = new OutboxMessage(

                dto.MessageId,

                dto.CorrelationId,

                _options.ExchangeName,

                _options.RoutingKeys.StockReservado,

                nameof(StockReservadoMessageDto),

                payload

                );



            _outboxRepositorio.Insertar(

                outbox

                );

        }

    }
}
