namespace AmetisAPI.API.Presenters
{
    /// <summary>Présente la valeur d'une <see cref="ValueResponse{T}"/> en JSON, ou ses erreurs avec le statut HTTP associé.</summary>
    public class ValuePresenter<T> : AResponseMessageJsonPresenter<ValueResponse<T>>
    {
        /// <inheritdoc />
        protected override object GetSuccessMember(ValueResponse<T> p_Response) => p_Response.Value;
    }
}
