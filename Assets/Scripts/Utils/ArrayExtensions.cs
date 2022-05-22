namespace System
{
    public static class ArrayExtensions
    {
        public static bool ValidateRange<T>(this T[] arr, int index)
        {
            return index >= 0 && index <= arr.Length;
        }

        public static void AssertRange<T>(this T[] arr, int index)
        {
            if (arr.ValidateRange(index) == false)
            {
                throw new IndexOutOfRangeException($"{index} was out of range {{0, {arr.Length}}}");
            }
        }
        
        public static void Insert<T>(this T[] arr, T element, int index)
        {
            arr.AssertRange(index);

            for (int i = arr.Length - 1; i > index; i--)
            {
                arr[i] = arr[i - 1];
            }

            arr[index] = element;
        }

        public static void Remove<T>(this T[] arr, int index)
        {
            arr.Pop(index);
        }

        public static T Pop<T>(this T[] arr, int index)
        {
            arr.AssertRange(index);

            T element = arr[index];
            for (int i = index; i < arr.Length; i++)
            {
                if (i == arr.Length - 1)
                {
                    arr[i] = default;
                    break;
                }

                arr[i] = arr[i + 1];
            }

            return element;
        }

        public static void Move<T>(this T[] arr, int from, int to)
        {
            arr.AssertRange(from);
            arr.AssertRange(to);

            int factor = from > to ? -1 : 1;

            var tmp = arr[from];
            for (int i = from; i > to; i += factor)
            {
                arr[i] = arr[i + factor];
            }
            arr[to] = tmp;
        }

        public static void Swap<T>(this T[] arr, int i1, int i2)
        {
            arr.AssertRange(i1);
            arr.AssertRange(i2);

            var tmp = arr[i1];
            arr[i1] = arr[i2];
            arr[i2] = tmp;
        }

        public static void Clear<T>(this T[] arr, int index = 0)
        {
            Array.Clear(arr, index, arr.Length);
        }

        public static void Clear<T>(this T[] arr, int index, int length)
        {
            Array.Clear(arr, index, length);
        }
    }
}