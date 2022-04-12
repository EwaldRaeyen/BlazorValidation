Blazor Validation with FluentValidation

Steps
1. Create model
2. Create validator for model
3. Register model validator as a singleton service
4. Inside EditForm place following code:
    <Validator ValidatorType=typeof(ModelValidator) ValidateModelOnFieldChange=true OnModelValidation="OnModelValidationStatusChange" />
5. For Input fields set Value, ValueExpression
6. For Input field add an EventHandler for ValueChanged, where you:
    Set the model its property with new value ~ model.SetProperty(propertyPath, value)
    Notify the context of a field change ~ context.NotifyFieldChanged(FieldIdentifierHelper.ToFieldIdentifier(context, propertyPath))
7. on _Imports.razor add:
    @using BlazorValidation

full example:

Model
-----
```public class Habitant
```{
```    public string FirstName { get; set; }
```    public string LastName { get; set; }
```    public Address Address { get; set; }
```}

```public class Address
```{
```    public string Country { get; set; }

```    public string City { get; set; }

```    public string ZipCode { get; set; }

```    public string Street { get; set; }

```    public string Number { get; set; }

```    public string Extension { get; set; }
```}

Validators
---------
```public class HabitantValidator : AbstractValidator<Habitant>
```{
```    public HabitantValidator()
```    {
```        RuleFor(r => r.FirstName).NotEmpty();
```        RuleFor(r => r.LastName).NotEmpty();
```        RuleFor(r => r.Address).SetValidator(new AddressValidator());
```    }
```}
  
```public class AddressValidator : AbstractValidator<Address>
```{
```    public AddressValidator()
```    {
```        RuleFor(x => x.Street).NotEmpty();
```        RuleFor(x => x.Number).NotEmpty();
```        RuleFor(x => x.ZipCode).NotEmpty();
```        RuleFor(x => x.City).NotEmpty();
```        RuleFor(x => x.Country).NotEmpty();
```    }
```}

FORM:
-----
````<EditForm Model=@_habitant OnValidSubmit="Save">
````    <Validator ValidatorType=typeof(HabitantValidator) ValidateModelOnFieldChange=true OnModelValidation="OnModelValidationStatusChange" />
````    <div class="row">
````        <label>FirstName</label>
````        <InputText Value="@_habitant.FirstName" ValueChanged="@((value) => OnChange(value, nameof(_habitant.FirstName), context))" ValueExpression="@(() => _habitant.FirstName)" class="form-control" />
````    </div>
````    <div class="row">
````        <label>LastName</label>
````        <InputText Value="@_habitant.LastName" ValueChanged="@((value) => OnChange(value, nameof(_habitant.LastName), context))" ValueExpression="@(() => _habitant.LastName)" class="form-control" />
````    </div>
````    <hr />
````    <div class="row">
````        <label>Street</label>
````        <InputText Value="@_habitant.Address.Street" ValueChanged="@((value) => OnStreetChange(value, "Address.Street", context))" ValueExpression="@(() => _habitant.Address.Street)" class="form-control" />
````    </div>
````    <div class="row">
````        <label>Number</label>
````        <InputText Value="@_habitant.Address.Number" ValueChanged="@((value) => OnChange(value, "Address.Number", context))" ValueExpression="@(() => _habitant.Address.Number)" class="form-control" />
````    </div>
````    <div class="row">
````        <label>ZipCode</label>
````        <InputText Value="@_habitant.Address.ZipCode" ValueChanged="@((value) => OnChange(value, "Address.ZipCode", context))" ValueExpression="@(() => _habitant.Address.ZipCode)" class="form-control" />
````    </div>
````    <div class="row">
````        <label>City</label>
````        <InputText Value="@_habitant.Address.City" ValueChanged="@((value) => OnChange(value, "Address.City", context))" ValueExpression="@(() => _habitant.Address.City)" class="form-control" />
````    </div>
````    <div class="row">
````        <label>Country</label>
````        <InputText Value="@_habitant.Address.Country" ValueChanged="@((value) => OnChange(value, "Address.Country", context))" ValueExpression="@(() => _habitant.Address.Country)" class="form-control" />
````    </div>
````
````    <button type="submit" disabled="@_btnDisabled">save</button>
````</EditForm>
````
````@code {
````    private Habitant _habitant = new Habitant { Address = new Address() };
````    private bool _btnDisabled = true;
````
````    private async Task Save()
````    {
````        await Task.Delay(100);
````    }
````
````    private void OnChange(object value, string propertyPath, EditContext context)
````    {
````        _habitant.SetProperty(propertyPath, value);
````        context.NotifyFieldChanged(FieldIdentifierHelper.ToFieldIdentifier(context, propertyPath));
````        StateHasChanged();
````    }
````
````    private void OnStreetChange(object value, string propertyName, EditContext context)
````    {
````        _habitant.Address.Street = (string)value;
````        context.NotifyFieldChanged(FieldIdentifierHelper.ToFieldIdentifier(context, propertyName));
````    }
````
````    private void OnModelValidationStatusChange(bool isValid)
````    {
````        _btnDisabled = !isValid;
````    }
````}
