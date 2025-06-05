namespace Common.Ddd.Domain.Values;

public class ErrorList : List<Error>
{
    public ErrorList() : base() { }
    public ErrorList(IEnumerable<Error> errors) : base(errors) { }
    public ErrorList(params Error[] errors) : base(errors) { }

    public string ErrorMessage => string.Join("; ", this.Select(e => e.Description));
}