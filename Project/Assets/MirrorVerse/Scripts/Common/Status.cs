using System;

namespace MirrorVerse
{
    // List of status codes copied from the originator of Status/StatusOr notions:
    // https://github.com/googleapis/googleapis/blob/master/google/rpc/code.proto
    public enum StatusCode : int
    {
        Ok = 0,
        Cancelled = 1,
        Unknown = 2,
        InvalidArgument = 3,
        DeadlineExceeded = 4,
        NotFound = 5,
        AlreadyExists = 6,
        PermissionDenied = 7,
        ResourceExhausted = 8,
        FailedPrecondition = 9,
        Aborted = 10,
        OutOfRange = 11,
        Unimplemented = 12,
        Internal = 13,
        Unavailable = 14,
        DataLoss = 15,
        Unauthenticated = 16,
    }

    // Status of a given operation.
    public struct Status
    {    
        private StatusCode _statusCode;
        private string _message;

        public Status(StatusCode statusCode = StatusCode.Ok, string message = null)
        {
            _statusCode = statusCode;
            _message = message;
        }

        public bool IsOk
        {
            get
            {
                return _statusCode == StatusCode.Ok;
            }
        }

        public StatusCode Code
        {
            get
            {
                return _statusCode;
            }
        }

        public string Message
        {
            get
            {
                return string.IsNullOrWhiteSpace(_message) ? "" : _message;
            }
        }

        public static implicit operator Status(StatusCode code)
        {
            return new Status(code);
        }

        public static Status Ok
        {
            get
            {
                return new Status(StatusCode.Ok);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_message))
            {
                return _statusCode.ToString();
            }
            return String.Format("{0} ({1})", _statusCode.ToString(), _message);
        }
    }

    // Carries both status code and value itself as a result of a given operation.
    public struct StatusOr<T>
    {
        Status _status;
        T _value;

        public StatusOr(T value) {
            _status = StatusCode.Ok;
            _value = value;
        }

        public StatusOr(Status status) {
            _status = status;
            _value = default(T);
        }

        public Status Status { 
            get
            {
                return _status;
            }
        }

        public bool HasValue
        {
            get
            {
                return _status.IsOk;
            }
        }

        public T Value
        {
            get
            {
                return _value;
            }
        }

        public StatusOr<F> Convert<F>(Func<T, F> func)
        {
            if (_status.IsOk)
            {
                return func(_value);
            }
            else
            {
                return _status;
            }
        }

        public T ValueOr(T defaultValue)
        {
            
            return !_status.IsOk ? defaultValue : _value;
        }

        public static implicit operator StatusOr<T>(T value)
        {
            return new StatusOr<T>(value);
        }

        public static implicit operator StatusOr<T>(Status status)
        {
            return new StatusOr<T>(status);
        }

        public static implicit operator StatusOr<T>(StatusCode statusCode)
        {
            return new StatusOr<T>(new Status(statusCode));
        }
    }
}
