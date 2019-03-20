using System;
using System.Collections.Generic;
using Tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturada;
        public bool xeque { get; private set; }
        public Peca enPassant { get; private set; }

        public PartidaDeXadrez()
        {
            tab = new tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Amarelo;
            terminada = false;
            xeque = false;
            enPassant = null;
            pecas = new HashSet<Peca>();
            capturada = new HashSet<Peca>();
            colocarPecas();
        }

        public Peca executaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQtdMovimento();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if (pecaCapturada != null)
            {
                capturada.Add(pecaCapturada);
            }

            //jogadaespecial roque pequeno
            if (p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQtdMovimento();
                tab.colocarPeca(T, destinoT);
            }

            //jogadaespecial roque grande
            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQtdMovimento();
                tab.colocarPeca(T, destinoT);
            }

            //jogadaespecial en passant
            if (p is Peao)
            {
                if (origem.Coluna != destino.Coluna && pecaCapturada == null)
                {
                    Posicao posP;
                    if (p.cor == Cor.Amarelo)
                    {
                        posP = new Posicao(destino.Linha + 1, destino.Coluna);
                    }
                    else
                    {
                        posP = new Posicao(destino.Linha - 1, destino.Coluna);
                    }
                    pecaCapturada = tab.retirarPeca(posP);
                    capturada.Add(pecaCapturada);
                }
            }

            return pecaCapturada;
        }

        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.retirarPeca(destino);
            p.decrementarQtdMovimento();
            if (pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturada.Remove(pecaCapturada);
            }
            tab.colocarPeca(p, origem);

            //jogadaespecial roque grande desfazer
            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = tab.retirarPeca(destinoT);
                T.incrementarQtdMovimento();
                tab.colocarPeca(T, origemT);
            }
            //jogadaespecial roque pequeno desfazer
            if (p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = tab.retirarPeca(destinoT);
                T.incrementarQtdMovimento();
                tab.colocarPeca(T, origemT);
            }

            //jogadaespecial en passant
            if (p is Peao)
            {
                if (origem.Coluna != destino.Coluna && pecaCapturada == enPassant)
                {
                    Peca peao = tab.retirarPeca(destino);
                    Posicao posP;
                    if (p.cor == Cor.Amarelo)
                    {
                        posP = new Posicao(3, destino.Coluna);
                    }
                    else
                    {
                        posP = new Posicao(4, destino.Coluna);
                    }
                    tab.colocarPeca(peao, posP);
                }
            }
        }

        public void realizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = executaMovimento(origem, destino);

            if (estaEmXeque(jogadorAtual))
            {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new EcessaoTabuleiro("Você não pode se colocar em xeque!");
            }

            Peca p = tab.peca(destino);

            //jogadaespecial promocao
            if (p is Peao)
            {
                if (p.cor == Cor.Amarelo && destino.Linha == 0 || (p.cor == Cor.Vermelho && destino.Linha == 7))
                {
                    p = tab.retirarPeca(destino);
                    pecas.Remove(p);
                    Peca dama = new Dama(tab, p.cor);
                    tab.colocarPeca(dama, destino);
                    pecas.Add(dama);
                }
            }
            if (estaEmXeque(adversaria(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }
            if (testeXequeMate(jogadorAtual))
            {
                terminada = true;
            }
            else
            {
                turno++;
                mudaJogador();
            }

            

            //jogadaespecial en passant
            if (p is Peao && (destino.Linha == origem.Linha - 2 || destino.Linha == origem.Linha + 2))
            {
                enPassant = p;
            }
            else
            {
                enPassant = null;
            }

        }

        public void validarPosicaoDeOrigem(Posicao pos)
        {
            if (tab.peca(pos) == null)
            {
                throw new EcessaoTabuleiro("Não existe peça na posição de origem escolhida! Enter para continuar...");
            }
            if (jogadorAtual != tab.peca(pos).cor)
            {
                throw new EcessaoTabuleiro("A peça de origem não é sua! Enter para continuar...");
            }
            if (!tab.peca(pos).existeMovimentoPossiveis())
            {
                throw new EcessaoTabuleiro("Não á possibilidade de movimento! Enter para continuar...");
            }
        }

        public void validarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!tab.peca(origem).movimentoPossivel(destino))
            {
                throw new EcessaoTabuleiro("Posição de destino invalida! Enter para continuar...");
            }
        }

        private void mudaJogador()
        {
            if (jogadorAtual == Cor.Amarelo)
            {
                jogadorAtual = Cor.Vermelho;
            }
            else
            {
                jogadorAtual = Cor.Amarelo;
            }
        }

        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in capturada)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> pecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(pecasCapturadas(cor));
            return aux;
        }

        private Cor adversaria(Cor cor)
        {
            if (cor == Cor.Amarelo)
            {
                return Cor.Vermelho;
            }
            else
            {
                return Cor.Amarelo;
            }
        }

        private Peca rei(Cor cor)
        {
            foreach (Peca x in pecasEmJogo(cor))
            {
                if (x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        public bool estaEmXeque(Cor cor)
        {
            Peca R = rei(cor);
            if (R == null)
            {
                throw new EcessaoTabuleiro("Não existe rei" + cor + "no tabuleiro!");
            }
            foreach (Peca x in pecasEmJogo(adversaria(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();
                if (mat[R.posicao.Linha, R.posicao.Coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool testeXequeMate(Cor cor)
        {
            if (!estaEmXeque(cor))
            {
                return false;
            }
            foreach (Peca x in pecasEmJogo(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for (int i = 0; i < tab.linhas; i++)
                {
                    for (int j = 0; j < tab.colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = executaMovimento(origem, destino);
                            bool testeXeque = estaEmXeque(cor);
                            desfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void colocarNovaPeca(char coluna, int linha, Peca peca)
        {
            tab.colocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            pecas.Add(peca);
        }

        private void colocarPecas()
        {
            colocarNovaPeca('a', 1, new Torre(tab, Cor.Amarelo));
            colocarNovaPeca('b', 1, new Cavalo(tab, Cor.Amarelo));
            colocarNovaPeca('c', 1, new Bispo(tab, Cor.Amarelo));
            colocarNovaPeca('d', 1, new Dama(tab, Cor.Amarelo));
            colocarNovaPeca('e', 1, new Rei(tab, Cor.Amarelo, this));
            colocarNovaPeca('f', 1, new Bispo(tab, Cor.Amarelo));
            colocarNovaPeca('g', 1, new Cavalo(tab, Cor.Amarelo));
            colocarNovaPeca('h', 1, new Torre(tab, Cor.Amarelo));
            colocarNovaPeca('a', 2, new Peao(tab, Cor.Amarelo, this));
            colocarNovaPeca('b', 2, new Peao(tab, Cor.Amarelo, this));
            colocarNovaPeca('c', 2, new Peao(tab, Cor.Amarelo, this));
            colocarNovaPeca('d', 2, new Peao(tab, Cor.Amarelo, this));
            colocarNovaPeca('e', 2, new Peao(tab, Cor.Amarelo, this));
            colocarNovaPeca('f', 2, new Peao(tab, Cor.Amarelo, this));
            colocarNovaPeca('g', 2, new Peao(tab, Cor.Amarelo, this));
            colocarNovaPeca('h', 2, new Peao(tab, Cor.Amarelo, this));

            colocarNovaPeca('a', 8, new Torre(tab, Cor.Vermelho));
            colocarNovaPeca('b', 8, new Cavalo(tab, Cor.Vermelho));
            colocarNovaPeca('c', 8, new Bispo(tab, Cor.Vermelho));
            colocarNovaPeca('d', 8, new Dama(tab, Cor.Vermelho));
            colocarNovaPeca('e', 8, new Rei(tab, Cor.Vermelho, this));
            colocarNovaPeca('f', 8, new Bispo(tab, Cor.Vermelho));
            colocarNovaPeca('g', 8, new Cavalo(tab, Cor.Vermelho));
            colocarNovaPeca('h', 8, new Torre(tab, Cor.Vermelho));
            colocarNovaPeca('a', 7, new Peao(tab, Cor.Vermelho, this));
            colocarNovaPeca('b', 7, new Peao(tab, Cor.Vermelho, this));
            colocarNovaPeca('c', 7, new Peao(tab, Cor.Vermelho, this));
            colocarNovaPeca('d', 7, new Peao(tab, Cor.Vermelho, this));
            colocarNovaPeca('e', 7, new Peao(tab, Cor.Vermelho, this));
            colocarNovaPeca('f', 7, new Peao(tab, Cor.Vermelho, this));
            colocarNovaPeca('g', 7, new Peao(tab, Cor.Vermelho, this));
            colocarNovaPeca('h', 7, new Peao(tab, Cor.Vermelho, this));
        }
    }
}
