namespace Restaurante.Api.Response
{
    // T representa el tipo de datos que devolverá la respuesta.
    public class ApiResponse<T>
    {
        // Datos devueltos por la API.
        public T Data { get; set; }

        public ApiResponse(T data)
        {
            Data = data;
        }
    }
}