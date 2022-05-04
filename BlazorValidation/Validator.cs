using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BlazorValidation
{
    public class Validator : ComponentBase
    {
        [CascadingParameter]
        public EditContext EditContext { get; set; }

        [Parameter]
        public Type ValidatorType { get; set; }

        [Parameter]
        public bool ValidateModelOnFieldChange { get; set; }

        [Parameter]
        public ValidationMessageStore? ValidationMessageStore { get; set; }

        [Parameter]
        public EventCallback<bool> OnModelValidation { get; set; }

        private IValidator _validator;
        private ValidationMessageStore _validationMessageStore;
        private readonly char[] _separators = new[] { '.', '[' };

        [Inject]
        private IServiceProvider ServiceProvider { get; set; }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            EditContext previousEditContext = EditContext;
            Type previousValidatorType = ValidatorType;

            await base.SetParametersAsync(parameters);

            if (EditContext == null)
                throw new NullReferenceException($"{nameof(Validator)} must be placed within an {nameof(EditForm)}");

            if (ValidatorType == null)
                throw new NullReferenceException($"{nameof(ValidatorType)} must be specified.");

            if (!typeof(IValidator).IsAssignableFrom(ValidatorType))
                throw new ArgumentException($"{ValidatorType.Name} must implement {typeof(IValidator).FullName}");

            if (ValidatorType != previousValidatorType)
                ValidatorTypeChanged();

            if (EditContext != previousEditContext)
                EditContextChanged();
        }

        private void ValidatorTypeChanged()
        {
            _validator = (IValidator)ServiceProvider.GetService(ValidatorType);
        }

        void EditContextChanged()
        {
            if (ValidationMessageStore == null)
                ValidationMessageStore = new ValidationMessageStore(EditContext);
            HookUpEditContextEvents();
        }

        private void HookUpEditContextEvents()
        {
            EditContext.OnValidationRequested += ValidationRequested;
            EditContext.OnFieldChanged += FieldChanged;
        }

        private void ValidationRequested(object sender, ValidationRequestedEventArgs args)
        {
            _validationMessageStore.Clear();
            var model = EditContext.Model;
            ValidationRequestedRec(model);
        }

        private void ValidationRequestedRec(object model)
        {
            var validator = GetValidatorForModel(model);

            var result = validator.Validate(new ValidationContext<object>(model));
            foreach (var error in result.Errors)
            {
                var fieldIdentifier = FieldIdentifierHelper.ToFieldIdentifier(EditContext, error.PropertyName);
                _validationMessageStore.Add(fieldIdentifier, error.ErrorMessage);
            }

            var members = model.GetType().GetMembers(BindingFlags.GetProperty);
            foreach (var member in members)
            {
                if (member.GetType().IsClass)
                {
                    FieldInfo? field = model.GetType().GetField(member.Name);
                    object? fieldValue = field?.GetValue(model);
                    if (fieldValue != null)
                    {
                        ValidationRequestedRec(fieldValue);
                    }
                }
            }
        }

        private bool ValidateRec(object model)
        {
            var validator = GetValidatorForModel(model);
            var result = validator.Validate(new ValidationContext<object>(model));

            if (result.Errors.Any())
            {
                return false;
            }

            var members = model.GetType().GetMembers(BindingFlags.GetProperty);
            var validMembers = true;
            for (int i = 0; i < members.Length && validMembers; i++)
            {
                var member = members.ElementAt(i);
                if (member.GetType().IsClass)
                {
                    FieldInfo? field = model.GetType().GetField(member.Name);
                    object? fieldValue = field?.GetValue(model);

                    if (fieldValue != null)
                    {
                        validMembers = ValidateRec(fieldValue);
                    }
                }
            }
            return validMembers;
        }

        private void AddValidationResult(object model, ValidationResult validationResult)
        {
            foreach (ValidationFailure error in validationResult.Errors)
            {
                var fieldIdentifier = new FieldIdentifier(model, error.PropertyName);
                _validationMessageStore.Add(fieldIdentifier, error.ErrorMessage);
            }
            EditContext.NotifyValidationStateChanged();
        }

        private async void FieldChanged(object sender, FieldChangedEventArgs args)
        {
            FieldIdentifier fieldIdentifier = args.FieldIdentifier;
            _validationMessageStore.Clear(fieldIdentifier);

            var propertiesToValidate = new string[] { fieldIdentifier.FieldName };
            var validator = GetValidatorForField(fieldIdentifier);
            var validatorSelector = new MemberNameValidatorSelector(propertiesToValidate);
            var propertyChain = new PropertyChain();
            var fluentValidationContext = new ValidationContext<object>(fieldIdentifier.Model, propertyChain, validatorSelector);

            ValidationResult result = await validator.ValidateAsync(fluentValidationContext);

            if (ValidateModelOnFieldChange)
            {
                await this.OnModelValidation.InvokeAsync(ValidateRec(EditContext.Model));
            }

            AddValidationResult(fieldIdentifier.Model, result);
        }

        private object GetModelOfField(FieldIdentifier fieldIdentifier)
        {
            return fieldIdentifier.Model;
        }

        private IValidator GetValidatorForField(FieldIdentifier fieldIdentifier)
        {
            var model = GetModelOfField(fieldIdentifier);
            return GetValidatorForModel(model);
        }

        private IValidator GetValidatorForModel(object model)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
            if (ServiceProvider != null)
            {
                try
                {
                    if (ServiceProvider.GetService(validatorType) is IValidator validator)
                    {
                        return validator;
                    }
                }
                catch (Exception)
                {
                }
            }

            throw new Exception($"Validator for model of type {model.GetType()} not found.");
        }
    }
}
