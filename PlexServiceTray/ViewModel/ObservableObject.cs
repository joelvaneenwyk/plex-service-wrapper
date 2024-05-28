using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace PlexServiceTray.ViewModel
{
    [PublicAPI]
    public abstract class ObservableObject : INotifyPropertyChanged, IDataErrorInfo
    {
        protected object? ValidationContext { get; init; }

        internal bool _isSelected;

        public virtual bool IsSelected
        {
            get => _isSelected;
            set 
            {
                if (_isSelected == value) return;

                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private bool _isExpanded;

        public bool IsExpanded
        {
            set 
            {
                if (_isExpanded == value) return;

                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// This is required to create on property changed events
        /// </summary>
        /// <param name="name">What property of this object has changed</param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
            if (Validators.ContainsKey(name))
                UpdateError();
        }

        private Dictionary<string, object?> PropertyGetters
        {
            get
            {
                return GetType().GetProperties().Where(p => GetValidations(p).Length != 0).ToDictionary(p => p.Name, GetValueGetter);
            }
        }

        private Dictionary<string, ValidationAttribute[]> Validators
        {
            get
            {
                return GetType().GetProperties().Where(p => GetValidations(p).Length != 0).ToDictionary(p => p.Name, GetValidations);
            }
        }

        private static ValidationAttribute[] GetValidations(PropertyInfo property)
        {
            return (ValidationAttribute[])property.GetCustomAttributes(typeof(ValidationAttribute), true);
        }

        private object? GetValueGetter(PropertyInfo property)
        {
            return property.GetValue(this, null);
        }

        private string _error = string.Empty;

        public string Error => _error;

        private void UpdateError()
        {
            var errors = from i in Validators
                         from v in i.Value
                         where !Validate(v, PropertyGetters[i.Key])
                         select v.ErrorMessage;
            _error = string.Join(Environment.NewLine, errors.ToArray());
            OnPropertyChanged(nameof(Error));
        }

        public string this[string columnName]
        {
            get
            {
                if (PropertyGetters.TryGetValue(columnName, out object? value))
                {
                    string?[] errors = Validators[columnName].Where(v => !Validate(v, value))
                        .Select(v => v.ErrorMessage).ToArray();
                    OnPropertyChanged(nameof(Error));
                    return string.Join(Environment.NewLine, errors);
                }

                OnPropertyChanged(nameof(Error));
                return string.Empty;
            }
        }

        private bool Validate(ValidationAttribute v, object? value)
        {
            ArgumentNullException.ThrowIfNull(ValidationContext, nameof(ValidationContext));
            ValidationResult? result = v.GetValidationResult(
                value, new ValidationContext(ValidationContext, null, null));
            return result == ValidationResult.Success;
        }
    }
}
