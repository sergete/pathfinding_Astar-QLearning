using Assets.Scripts.DataStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QLearning {

    public int seed { get; set; }
    bool[,] updatedTile;
    float[,] Qtable;
    float[,] RewardTable { get; set; }
    int action = -1;
    float alfa = 0.5f;
    float gamma = 0.9f; // factor de descuento para Q-target.
    float e;            // Valor inicial de epsilon.
    float eMin = 0.1f; // Minimo valor de epsilon.
    int lastState;
    public int maxIterations { get; set; }

    //Constructor del objeto QLearning
    public QLearning(BoardInfo board, int seed)
    {
        Qtable = new float[board.NumColumns, board.NumRows];
        RewardTable = new float[board.NumColumns, board.NumRows];
        updatedTile = new bool[board.NumColumns, board.NumRows];
        this.seed = seed;
        initQtable();
        initRewardTable(board);
        maxIterations = 600;
        e = 1.0f;
    }
    //Método get del epsilon actual
    public float getE()
    {
        return e;
    }
    //Método set para actualizar el epsilon actual si llega a 0.1 no lo actualizaremos más
    public void setE(float dicrease)
    {
        if(e > eMin)
        {
            e -= dicrease;
        }
        else
        {
            e = eMin;
        }
        
    }
    //Inicializamos la tabla Q a 0
    private void initQtable()
    {
        for(int i = 0; i<Qtable.GetLength(0); i++)
        {
            for(int j = 0; j < Qtable.GetLength(1); j++)
            {
                Qtable[i, j] = 0;
            }
        }
    }
    /*
     * Inicializamos la tabla de recompensas a -1 las casillas atravesables
     * -5 las celdas con muros
     * 100 la meta
     */
    private void initRewardTable(BoardInfo board)
    {
        //Obtenemos toda la información de todas las celdas del tablero
        CellInfo[,] info = board.CellInfos;
        //Celda con objeto Goal
        CellInfo goal = board.CellWithItem("Goal");

        for (int i = 0; i < RewardTable.GetLength(0); i++)
        {
            for (int j = 0; j < RewardTable.GetLength(1); j++)
            {
                if (info[i, j].GetPosition == goal.GetPosition)
                {
                    RewardTable[i, j] = 100;
                }
                else if (!info[i,j].Walkable)
                {
                    RewardTable[i, j] = -5;
                }
                else
                {
                    RewardTable[i, j] = -1;
                }
            }
        }
    }

    /*
     * Nos devuelve el siguiente paso que se realizará
     */
    public int nextStep(CellInfo currentPos, BoardInfo board)
    {
        //Nos da un número al azar distinto cada vez así nos aseguramos que el random no es el mismo siempre
        System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
        //Paso por defecto -1
        var next = -1;
        //Número entre 0 y 1
        var step = rand.NextDouble();
        //En caso de que el número aleatorio generado entre 0 y 1 sea mayor que epsilon calculamos el mayor de los vecinos
        if(step > e)
        {
            //Calculamos el siguiente paso según la Qtable.
            next = MaxValueClose(currentPos, board);
        }
        //En caso de que el número aleatorio sea menor Calculamos el random entre 0 y 4 para saber a que casilla va a moverse
        else
        {
            //Metodo que calcula la posición aleatoria
            next = RandomStep(currentPos, board);
        }

        //me muevo arriba si next == 0 y la fila en la que nos encontramos es < al total de filas - 1
        if (next == 0 && currentPos.RowId < Qtable.GetLength(1) - 1)
        {
            //Según el valor next y donde me voy a mover calculo la posición futura 
            Vector2 nextStep = new Vector2(currentPos.ColumnId, currentPos.RowId + 1);
            //Actualizamos la QTable pasando como parámetro la posición actual y la posición futura
            learn(currentPos, nextStep, board);
            //Si el siguiente paso es igual a -5 quiere decir que es una casilla no atravesable y devolveremos -1 que significa que el siguiente paso es None
            //Sino devuelve el valor de next
            if (RewardTable[(int)nextStep.x, (int)nextStep.y] == -5)
            {
                return -1;
            }
            else
            {
                return next;
            }
        }
        //me muevo abajo si el valor de next == 1 y la fila actual es > a 0
        else if (next == 1 && currentPos.RowId > 0)
        {
            //Según el valor next y donde me voy a mover calculo la posición futura 
            Vector2 nextStep = new Vector2(currentPos.ColumnId, currentPos.RowId - 1);
            //Actualizamos la QTable pasando como parámetro la posición actual y la posición futura
            learn(currentPos, nextStep, board);
            //Si el siguiente paso es igual a -5 quiere decir que es una casilla no atravesable y devolveremos -1 que significa que el siguiente paso es None
            //Sino devuelve el valor de next
            if (RewardTable[(int)nextStep.x, (int)nextStep.y] == -5)
            {
                return -1;
            }
            else
            {
                return next;
            }
        }
        //me muevo a la izquierda si la columna es mayor a 0
        else if (next == 2 && currentPos.ColumnId > 0)
        {
            //Según el valor next y donde me voy a mover calculo la posición futura 
            Vector2 nextStep = new Vector2(currentPos.ColumnId -1, currentPos.RowId);
            //Actualizamos la QTable pasando como parámetro la posición actual y la posición futura
            learn(currentPos, nextStep, board);
            //Si el siguiente paso es igual a -5 quiere decir que es una casilla no atravesable y devolveremos -1 que significa que el siguiente paso es None
            //Sino devuelve el valor de next
            if (RewardTable[(int)nextStep.x, (int)nextStep.y] == -5)
            {
                return -1;
            }
            else
            {             
                return next;
            }
        }
        //me muevo a la derecha si la columna es < al total de columnas -1
        else if (next == 3 && currentPos.ColumnId < Qtable.GetLength(0) - 1)
        {
            //Según el valor next y donde me voy a mover calculo la posición futura 
            Vector2 nextStep = new Vector2(currentPos.ColumnId +1, currentPos.RowId);
            //Actualizamos la QTable pasando como parámetro la posición actual y la posición futura
            learn(currentPos, nextStep, board);
            //Si el siguiente paso es igual a -5 quiere decir que es una casilla no atravesable y devolveremos -1 que significa que el siguiente paso es None
            //Sino devuelve el valor de next
            if (RewardTable[(int)nextStep.x, (int)nextStep.y] == -5)
            {
                return -1;
            }
            else
            {
                return next;
            }
        }
        else
        {
            return -1;
        }

    }

    //Calcula el máximo valor cercano dentro de Qtable
    private int MaxValueClose(CellInfo currentPos, BoardInfo board)
    {
        //Obtenemos los vecinos de la posición actual
        CellInfo[] neighbours = currentPos.WalkableNeighbours(board);
        float maxvalue = 0;
        bool first = false;
        int pos = -1;
        //Vector2 ant = new Vector2(AnteriorPosX, AnteriorPosY);
        for(int i = 0; i < neighbours.Length; ++i)
        {
            if(neighbours[i] != null)
            {
                //Le decimos que al menos coja la primera posición de los vecinos y almacenamos su posición
                //y cambiamos el buleano first a true para que no vuelva a entrar
                if (!first)
                {
                    maxvalue = Qtable[neighbours[i].ColumnId, neighbours[i].RowId];
                    pos = i;
                    first = true;
                }
                //Si el valor de Qtable en la posición de neighbours[i] es mayor a maxvalue, actualizamos maxvalue y guardamos la posición
                if (Qtable[neighbours[i].ColumnId, neighbours[i].RowId] > maxvalue)
                    {
                        maxvalue = Qtable[neighbours[i].ColumnId, neighbours[i].RowId];
                        pos = i;
                        //Debug.Log("Vecino elegido " + i + " Columna: " + neighbours[i].ColumnId + " fila: " + neighbours[i].RowId);
                    }              
            }
               
        }
        //Si pos == -1 devuelve -1 y no haría nada, aquí no vamos a entrar pero está concebido como elemento de seguridad
        if(pos == -1)
        {
            return -1;
        }
        else
        {
            /*
             * En este paso lo que hacemos es calcular en que dirección nos hemos movido para retornar el valor exacto hacia donde debemos movernos
             */
            int columnpos = currentPos.ColumnId - neighbours[pos].ColumnId;
            int filapos = currentPos.RowId - neighbours[pos].RowId;
            //Nos movemos en horizontal
            if(columnpos != 0)
            {
                if (columnpos > 0)
                {
                    return 2;
                    //Debug.Log("Nos movemos a la iquierda");
                }

                else
                {
                    return 3;
                    //Debug.Log("Nos movemos a la derecha");
                }
                    
            }
            //Nos movemos en vertical
            else
            {
                if(filapos > 0)
                {
                    return 1;
                    //Debug.Log("Nos movemos abajo");
                }
                else
                {
                    return 0;
                    //Debug.Log("Nos movemos arriba");
                }
            }
        }

    }

    //Calcula un valor random que no nos dirija a muros sino a casillas transitables
    private int RandomStep(CellInfo currentPos, BoardInfo board)
    {
        //Genera una semilla distinta cada vez, así evitamos patrones de movimiento
        System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
        //paso inicial = -1
        int next = -1;
        bool validstep = false;
        //pasos posibles
        CellInfo[] possibleSteps = currentPos.WalkableNeighbours(board);

        if (possibleSteps.Length > 0)
        {
            //Permaneceremos en el bucle mientras el valor del array en la posición generada por Random no sea válida
            do
            {
                //random entre 0 y 4
                next = rand.Next(0, 4);
               //Si la posición del array no es null el movimiento es válido
                if (possibleSteps[next] != null)
                {
                        validstep = true;
                    
                }
            } while (!validstep);

        }
        else
        {
            return -1;
        }

        /*
        * En este paso lo que hacemos es calcular en que dirección nos hemos movido para retornar el valor exacto hacia donde debemos movernos
        */
        int columnpos = currentPos.ColumnId - possibleSteps[next].ColumnId;
        int filapos = currentPos.RowId - possibleSteps[next].RowId;
        if (columnpos != 0)
        {
            if (columnpos > 0)
            {
                return 2;
                //Debug.Log("Nos movemos a la iquierda");
            }

            else
            {
                return 3;
                //Debug.Log("Nos movemos a la derecha");
            }

        }
        else
        {
            if (filapos > 0)
            {
                return 1;
                //Debug.Log("Nos movemos abajo");
            }
            else
            {
                return 0;
                //Debug.Log("Nos movemos arriba");
            }
        }
    }

    /*
     * Este método lo que hace es actualizar Qtable teniendo en cuenta la posición actual y la acción siguiente y aplicando la fórmula
     * que tenemos en los apuntes para QLearning
     */
    private void learn(CellInfo currentPos, Vector2 nextPos, BoardInfo board)
    {
        Vector2 row = currentPos.GetPosition - nextPos;
        float currentValue = Qtable[(int)currentPos.GetPosition.x, (int)currentPos.GetPosition.y];
        float recompensa = RewardTable[(int)nextPos.x, (int)nextPos.y];
        float maxvalue = Qtable[(int)currentPos.GetPosition.x, (int)currentPos.GetPosition.y]; 
        CellInfo[] step = currentPos.WalkableNeighbours(board);
        int pos = -1;

        /*
         * Como pasamos como parámetro un vector2 y necesitamos expandir la siguiente posición, tenemos que buscar entre las celdas
         * vecinas de la posición actual aquella que coincida con la siguiente posición y guardamos su posición, así obtendremos una 
         * variable de tipo CellInfo que nos permitirá expandir sus vecinos que serán los valores para la acción siguiente
         */
        for(int i=0; i<step.Length; i++)
        {
            if(step[i] != null)
            {
                if(step[i].GetPosition == nextPos)
                {
                    pos = i;
                }
            }
        }
        //Array con los vecinos de la siguiente posición (nextPos)
        CellInfo[] nuevostep = step[pos].WalkableNeighbours(board);

        //Recorro el array nuevoStep que consideramos son las posibles acciones que puede realizar y obtenemos el valor máximo    
        for (int i = 0; i < nuevostep.Length; i++)
            {
                if(nuevostep[i] != null)
                {
                    if (Qtable[nuevostep[i].ColumnId, nuevostep[i].RowId] > maxvalue)
                    {
                        maxvalue = Qtable[nuevostep[i].ColumnId, nuevostep[i].RowId];
                    }
                }           
            }
            //Aplicamos la fórmula y utilizamos la posición nextPos de la Qtable con el valor calculado
            float updatedValue = (1-alfa)*currentValue + alfa * (recompensa + (gamma * maxvalue));
            Qtable[(int)nextPos.x, (int)nextPos.y] = updatedValue;
                             

    }

    //Imprime la tabla Qtable y el epsilon actual
    public void printTable()
    {
        Debug.Log("Epsilon actual: " + e);
        for (int i = 0; i < Qtable.GetLength(0); i++)
        {
            for (int j = 0; j < Qtable.GetLength(1); j++)
            {
                Debug.Log("Qtable: " + i + ", " + j + " valor: " + Qtable[i, j]);
                Debug.Log("RewardTable: " + i + ", " + j + " valor: " + RewardTable[i, j]);
            }
        }
    }

}
