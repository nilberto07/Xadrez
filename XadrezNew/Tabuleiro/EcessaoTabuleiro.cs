using System;

namespace Tabuleiro
{
    class EcessaoTabuleiro : Exception
    {
        public EcessaoTabuleiro(string msg) : base(msg)
        {
        }
    }
}
