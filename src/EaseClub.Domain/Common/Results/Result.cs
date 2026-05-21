using EaseClub.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EaseClub.Domain.Common.Results
{
    public sealed class Result<T> : IResult<T>
    {
        private readonly T? _Value=default;

        private readonly List<Error>? _Errors = null;

        public bool IsSuccess { get; }

        public bool IsError => !IsSuccess;

        public Error TopError => IsError ? _Errors!.First() : default;

        public  T Value => IsSuccess ? _Value! : throw new InvalidOperationException("Cannot access Value on an error result.");

        public IReadOnlyList<Error>? Errors => IsError ? _Errors!.AsReadOnly() : Array.Empty<Error>();


//---------------------------Constuctors--------------------------//       

        private Result(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            IsSuccess = true;
            _Value = value;
            
        }

        private Result(Error error)
        {
           
            IsSuccess=false;
            _Errors = new List<Error> { error};
        }

        private Result(List<Error> errors)
        {
            if (errors == null || !errors.Any())
                throw new ArgumentNullException(nameof(errors));
            IsSuccess = false;
            _Errors = errors;
        }

//---------------------------Implicit Operators--------------------------//

        public static implicit operator Result<T>(T value) => new Result<T>(value);
        public static implicit operator Result<T>(Error error) => new Result<T>(error);
        public static implicit operator Result<T>(List<Error> errors) => new Result<T>(errors);


//---------------------------Match Method--------------------------//
        public TNextValue Match<TNextValue>(Func<T, TNextValue> onSuccess, Func<List<Error>, TNextValue> onError)
        {
            if (IsSuccess)
                return onSuccess(_Value!);
            else
                return onError(_Errors!);
        }
    }

    public static class Result
    {
        public static Success Success => default;
        public static Created Created => default;
        public static Updated Updated => default;
        public static Deleted Deleted => default;

    }

    public readonly record struct Success;
    public readonly record struct Created;
    public readonly record struct Updated;
    public readonly record struct Deleted;

}
