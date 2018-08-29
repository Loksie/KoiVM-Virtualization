namespace KoiVM.Confuser
{
    internal class RC4
    {
        // Adopted from BouncyCastle

        private static readonly int STATE_LENGTH = 256;

        private readonly byte[] engineState;
        private byte[] workingKey;
        private int x;
        private int y;

        public RC4(byte[] key)
        {
            workingKey = (byte[]) key.Clone();

            x = 0;
            y = 0;

            if(engineState == null) engineState = new byte[STATE_LENGTH];

            // reset the state of the engine
            for(var i = 0; i < STATE_LENGTH; i++) engineState[i] = (byte) i;

            var i1 = 0;
            var i2 = 0;

            for(var i = 0; i < STATE_LENGTH; i++)
            {
                i2 = ((key[i1] & 0xff) + engineState[i] + i2) & 0xff;
                // do the byte-swap inline
                var tmp = engineState[i];
                engineState[i] = engineState[i2];
                engineState[i2] = tmp;
                i1 = (i1 + 1) % key.Length;
            }
        }

        public void Crypt(byte[] buf, int offset, int len)
        {
            for(var i = 0; i < len; i++)
            {
                x = (x + 1) & 0xff;
                y = (engineState[x] + y) & 0xff;

                // swap
                var tmp = engineState[x];
                engineState[x] = engineState[y];
                engineState[y] = tmp;

                // xor
                buf[i + offset] = (byte) (buf[i + offset]
                                          ^ engineState[(engineState[x] + engineState[y]) & 0xff]);
            }
        }
    }
}