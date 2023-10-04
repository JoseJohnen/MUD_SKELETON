namespace MUD_Skeleton.Commons.Auxiliary
{
    [Serializable]
    public class Pares<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Pares(T1 item1 = default, T2 item2 = default)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public Pares()
        {
        }

        #region ForEach Compatibility
        /*public IEnumerator GetEnumerator()
        {
            return (IEnumerator)this;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();*/
        #endregion
    }
}
