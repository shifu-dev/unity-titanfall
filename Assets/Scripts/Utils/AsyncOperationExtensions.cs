using System;
using System.Runtime.CompilerServices;

namespace UnityEngine
{
    public static class AsyncOperationExtensions
    {
        public static AsyncOperationAwaiter GetAwaiter(this AsyncOperation asyncOperation)
        {
            return new AsyncOperationAwaiter(asyncOperation);
        }

        public static AsyncOperationsAwaiter GetAwaiter(this AsyncOperation[] asyncOperations)
        {
            return new AsyncOperationsAwaiter(asyncOperations);
        }
    }

    public class AsyncOperationAwaiter : INotifyCompletion
    {
        private AsyncOperation asyncOperation;
        private Action continuation;

        public AsyncOperationAwaiter(AsyncOperation asyncOp)
        {
            this.asyncOperation = asyncOp;
            asyncOp.completed += OnRequestCompleted;
        }

        public bool IsCompleted => asyncOperation.isDone;

        public void GetResult() { }

        public void OnCompleted(Action continuation) => this.continuation = continuation;

        private void OnRequestCompleted(AsyncOperation asyncOperation) => continuation();
    }

    public class AsyncOperationsAwaiter : INotifyCompletion
    {
        private AsyncOperation[] asyncOperations;
        private uint count;
        private Action continuation;

        public AsyncOperationsAwaiter(AsyncOperation[] asyncOps)
        {
            count = 0;
            this.asyncOperations = asyncOps;
            foreach (var op in asyncOperations)
            {
                op.completed += OnRequestCompleted;
            }
        }

        public bool IsCompleted
        {
            get
            {
                foreach (var op in asyncOperations)
                {
                    if (op.isDone == false)
                        return false;
                }

                return true;
            }
        }

        public void GetResult() { }

        public void OnCompleted(Action continuation) => this.continuation = continuation;

        private void OnRequestCompleted(AsyncOperation asyncOperation)
        {
            count++;

            if (count == asyncOperations.Length)
            {
                continuation();
            }
        }
    }
}