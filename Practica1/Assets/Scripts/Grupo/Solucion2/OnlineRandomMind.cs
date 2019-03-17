using Assets.Scripts.DataStructures;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Assets.Scripts.Grupo
{
    public class OnlineRandomMind : AbstractPathMind
    {
        public Animator anim;
        List<Nodo> fathers = new List<Nodo>();
        List<Nodo> path = new List<Nodo>();
        List<Nodo> listofEnemies = new List<Nodo>();
        bool encontrado = false;
        EnemyBehaviour[] enemies;
        bool selected;
        bool allcatched;
        int numEnemies;
        int selectedEnemy;
        int contador;

        private void Start()
        {
            //guardamos en la variable enemyBehaviour los enemigos actuales
            enemies = FindObjectsOfType<EnemyBehaviour>();
            //guardamos la longitud del array para poder usar el método checkEnemies
            numEnemies = enemies.Length;
            //buleano que se activará cuando todos los enemigos hayan sido cogidos
            allcatched = false;
            //almacena la posición del enemigo seleccionada en el array enemies
            selectedEnemy = 0;
            //buleano que controla cuando un enemigo ha sido seleccionado para poder cogerle
            selected = false;
        }


        public override void Repath()
        {

        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            /*
            En las siguientes líneas calculamos el paso que se produce y en que dirección, por eso almacenamos la posición actual en currentPosX y currentPosY
            y la siguiente posición almacenada en la Lista fathers[0] que se guardará en futurePosX y futurePosY y si futurePosX es mayor o menor a currenPosX
            sabemos que se ha movido a derecha o izquierda, y si futurePosY es mayor o menor a currentPosY sabemos que se ha movido arriba o abajo que es el
            valor que retornaremos, en caso de que no coincida ningún valor o no se haya movido le decimos que no haga ningún movimiento 
            */
            float currentPosX = currentPos.GetPosition.x;
            float currentPosY = currentPos.GetPosition.y;
            float futurePosX;
            float futurePosY;

            //Si fathers > 0 todavía no hemos terminado de recorrer el camino
            if (fathers.Count > 0)
            {
                futurePosX = fathers[0].current.GetPosition.x;
                futurePosY = fathers[0].current.GetPosition.y;
            }
            //Si no hemos eliminado a todos los enemigos entramos aquí
            else if(!allcatched)
            {
                //Checkeamos si han sido todos los enemigos capturados
                allcatched = CheckEnemiesCatched();

                //Sino han sido capturados todos, no hemos seleccionado ningún enemigo y el número de enemigos es > 0 
                //seleccionamos el enemigo a capturar que esté más lejos de la meta
                if (!allcatched && !selected && numEnemies > 0)
                {
                    selectEnemy(goals);
                }
                /*
                 * -si hemos seleccionado el enemigo
                 * -no han sido todos los enemigos capturados
                 * -el número de enemigos es > 0
                 * -y la lista fathers es < 1 movimiento = este último parámetro viene determinado porque Como hemos programado un horizonte variable
                 *  en función de la distancia del enemigo al personaje no hacemos la siguiente llamada hasta que haya llegado a la casilla seleccionada,
                 *  cuando se encuentre lo suficientemente cerca cambiaremos a horizonte 1 para una mejor solución
                 */
                if (selected && !allcatched && numEnemies > 0 && fathers.Count < 1)
                {
                    CatchingEnemies(currentPos, goals, boardInfo);
                }
                //recorremos el camino
                if (fathers.Count() > 0 && !allcatched)
                    {
                        futurePosX = fathers[0].current.GetPosition.x;
                        futurePosY = fathers[0].current.GetPosition.y;
                    }
                 if (allcatched && !encontrado)
                {
                    calculatePath2(currentPos, goals, boardInfo);
                    futurePosX = fathers[0].current.GetPosition.x;
                    futurePosY = fathers[0].current.GetPosition.y;
                }
                 //No hacemos nada
                else
                    {
                        futurePosX = -1;
                        futurePosY = -1;
                    }
            }
            //Hemos llegado a la meta
            else
            {
                futurePosX = goals[0].GetPosition.x;
                futurePosY = fathers[0].current.GetPosition.y;
            }
            /*
             * Activamos las animaciones del pesonaje según la dirección en la que nos movamos y borramos la primera posición de 
             * la lista de padres (fathers) siempre que la lista sea > 0 hasta que lleguemos a la meta.
             */
            if (currentPosX > futurePosX && currentPosY == futurePosY)
            {
                anim.SetFloat("velocityX", -1f);
                anim.SetFloat("velocityY", 0f);
                if (fathers.Count > 0)
                    fathers.RemoveAt(0);
                return Locomotion.MoveDirection.Left;
            }
            else if (currentPosX < futurePosX && currentPosY == futurePosY)
            {
                anim.SetFloat("velocityX", 1f);
                anim.SetFloat("velocityY", 0f);
                if (fathers.Count > 0)
                    fathers.RemoveAt(0);
                return Locomotion.MoveDirection.Right;
            }
            else if (currentPosY > futurePosY && currentPosX == futurePosX)
            {
                anim.SetFloat("velocityY", -1f);
                anim.SetFloat("velocityX", 0f);
                if (fathers.Count > 0)
                    fathers.RemoveAt(0);
                return Locomotion.MoveDirection.Down;
            }
            else if (currentPosY < futurePosY && currentPosX == futurePosX)
            {
                anim.SetFloat("velocityY", 1f);
                anim.SetFloat("velocityX", 0f);
                if (fathers.Count > 0)
                    fathers.RemoveAt(0);
                return Locomotion.MoveDirection.Up;
            }
            else
            {

                anim.SetFloat("velocityX", 0f);
                anim.SetFloat("velocityY", 0f);
                if(fathers.Count > 0)
                    fathers.RemoveAt(0);    
                return Locomotion.MoveDirection.None;
            }
            
        }

        //Aquí aplicamos el algoritmo A* 
        public void calculatePath2(CellInfo currentPos, CellInfo[] goals, BoardInfo boardInfo)
        {
            //Lista de nodos expandidos
            List<Nodo> expandedNodos = new List<Nodo>();
            //Lista de nodos expandidos
            List<Nodo> abiertos = new List<Nodo>();
            //el bucle while se controla con el boolean encontrado2 que se activará cuando lleguemos a la meta y un contador
            // que tendrá un máximo de iteraciones de 1000 en caso de que no encontremos la meta por si el mapa fuera demasiado grande aunque sabemos que esto
            //nunca se cumplirá es más bien un elemento de seguridad
            bool encontrado2 = false;
            int contador = 0;

            //Creamos un objeto Nodo con la posición actual y lo almacenamos en la lista fathers
            Nodo current = new Nodo(currentPos, goals);
            fathers.Add(current);
            expandedNodos.Add(current);


            while (!encontrado2 && contador < 1000)
            {
                //Si el último Nodo añadido a la posición fathers es igual a la posición de la meta hemos encontrado la meta y calculamos el camino
                //accediendo a los padres de cada posición hasta llegar al inicio y así obtendríamos el camino hasta la meta.
                if (fathers[fathers.Count - 1].current.Equals(goals[0]))
                {
                    //Creamos una variable que nos servirá como contador para recorrer el array fathers
                    int cont = 0;
                    //Cambiamos a true los boolean para salir del bucle while y para decirle en GetNextMove que ya hemos encontrado la salida
                    encontrado2 = true;
                    encontrado = true;
                    //Insertamos en la lista path el último Nodo del array Fathers para recorrer la lista hacia atrás buscando al padre del último Nodo hasta llegar al inicio
                    path.Insert(0, fathers[fathers.Count - 1]);

                    while (cont < fathers.Count)
                    {
                        //sacamos el nodoPadre de la primera posición de path y la insertamos en la primera posición de la Lista path
                        Nodo nodoPadre = path[0].NodoPadre;
                        //verificamos que nodoPadre sea != null así sabremos que hemos llegado al inicio
                        if (nodoPadre != null)
                        {
                            path.Insert(0, nodoPadre);
                        }

                        cont++;
                    }
                    //copiamos path en fathers aquí ya estaría el camino completo
                    fathers = path;
                    //borramos la primera posición porque es la posición en la que nos encontramos
                    fathers.RemoveAt(0);
                }
                else
                {
                    //Expando el nodo padre último añadido
                    expandedNodos = fathers[fathers.Count - 1].expand(fathers[fathers.Count - 1], boardInfo, goals, fathers);
                    //Si los nodos expandidos son > 0
                    if (expandedNodos.Count == 0)
                    {
                        //Ordenar lista de abiertos por coste
                        IList<Nodo> ord = abiertos;
                        abiertos = ord.OrderBy(x => x.cost).ToList<Nodo>();
                        Debug.Log("No hay camino y tengo que coger abiertos Padre. Coste: " + fathers[fathers.Count - 1].cost + "Posición: " + fathers[fathers.Count - 1].current.GetPosition.ToString());
                        Debug.Log("No hay camino y tengo que coger abiertos. Coste: " + abiertos[0].cost + "Posición: " + abiertos[0].current.GetPosition.ToString());

                        //Si expandednodos está vacía pasamos el primero de la lista de abiertos y limpiamos expandedNodos y expandimos otra vez
                        expandedNodos.Clear();
                        if (abiertos.Count > 0)
                        {
                            fathers.Add(abiertos[0]);
                            abiertos.RemoveAt(0);
                        }

                        else
                        {
                            Debug.Log("No hay camino");
                            break;
                        }

                    }
                    else
                    {
                        //añadimos el primer Nodo de la lista ExpandedNodos que vendrán ordenador por coste de menor a mayor desde la clase Nodo
                        fathers.Add(expandedNodos[0]);
                        for (int i = 1; i < expandedNodos.Count; i++)
                        {
                            abiertos.Add(expandedNodos[i]);
                        }

                        //Ordenar lista de abiertos
                        IList<Nodo> ord = abiertos;
                        abiertos = ord.OrderBy(x => x.cost).ToList<Nodo>();
                        //Limpiamos expandedNodos
                        expandedNodos.Clear();
                    }
                } //Else
                contador++;
            }  //While
        }

        /*
         * Algoritmo con horizonte variable que depende de la distancia del personaje y el enemigo seleccionado
         */
        public void CatchingEnemies(CellInfo currentPos, CellInfo[] goals, BoardInfo boardInfo)
        {
            //creamos un Nodo con la posición actual y la añadimos a la lista fathers
            Nodo current = new Nodo(currentPos, goals);
            fathers.Add(current);
            //si el enemigo seleccionado es distinto de null y el número de enemigos es > 0
            if (enemies[selectedEnemy] != null && numEnemies > 0)
            {
                //max controla el horizonte, mínimo es 1
                int max = 1;
                //contador del bucle
                int contador = 0;
                //Lista de nodos expandidos
                List<Nodo> expandedNodos = new List<Nodo>();

                //Mientras contador sea menor al horizonte
                while (contador < max) { 
                
                //Expandimos los nodos de la última posición de la lista fathers que tienen el camino que estamos recorriendo
                expandedNodos = fathers[fathers.Count - 1].expand(fathers[fathers.Count - 1], boardInfo, goals, fathers);
                //Posicion por defecto -1, guardamos la posición del nodo expandido elegido
                int pos = -1;
                //distancia por defecto 200 ponemos una cantidad grande para que al menos guarde la primera posición
                float distance = 200;
                //Recorremos la lista de nodos expandidos
                for (int i = 0; i < expandedNodos.Count; i++)
                {
                    //Evitamos que pase por la meta en caso de que coincida
                    if (expandedNodos[i].current.GetPosition != goals[0].GetPosition)
                    {
                        //Distancia Manhattan desde cada nodo expandido hasta el enemigo seleccionado
                        float dis = Math.Abs(expandedNodos[i].current.ColumnId - enemies[selectedEnemy].CurrentPosition().ColumnId) + Math.Abs(expandedNodos[i].current.RowId - enemies[selectedEnemy].CurrentPosition().RowId);
                        //Si dis es menor a la distancia entramos en el if, actualizamos la distancia y la posición
                        if (dis < distance)
                        {
                            distance = dis;
                            pos = i;
                        }
                    }
                //Si distancia es > 3 el horizonte será la distancia -2 
                }
                if(distance > 3)
                    {
                        max = (int)distance - 2;
                    }
                //Sino horizonte = 1
                else
                    {
                        max = 1;
                    }
                //Añadimos el nodo expandido seleccionado
                fathers.Add(expandedNodos[pos]);
                //Limpiamos expanded nodos y aumentamos contador
                expandedNodos.Clear();
                    contador++;
                }
            }

        }

        //Comprobamos el número de enemigos capturados
        private bool CheckEnemiesCatched()
        {
            //Si es > 0 checkea enemigos y devuelve falso
            if(enemies.Count() > 0)
            {
                checkEnemies();
                return false;
            }
            //Sino checkea enemigos y devuelve true
            else
            {
                checkEnemies();
                return true;
            }
            
        }

        /*
         * Actualiza el número de enemigos que tenemos si ha cambiado actualizamos el estado y la variable numenemies
         * y la variable selected a false para que volvamos a seleccionar el siguiente enemigo a capturar o si todos han
         * sido eliminados
         */
        private void checkEnemies()
        {
            enemies = FindObjectsOfType<EnemyBehaviour>();
            if(enemies.Count() < numEnemies)
            {
                //fathers.Clear();
                numEnemies--;
                selected = false;
            }
        }

        //Seleccionamos el enemigo a capturar
        public void selectEnemy(CellInfo[] goals)
        {
            //Si el array enemies es > 0
            if (enemies.Length > 0)
            {
                //variable de la distancia
                float distance = -1;
                //inicializamos selectedEnemy a -1
                selectedEnemy = -1;

                //Recorremos el array de enemigos
                for (int i = 0; i < enemies.Count<EnemyBehaviour>(); i++)
                {
                    //Si es != null
                    if (enemies[i] != null)
                    {
                        //Distancia Manhattan desde la posición actual del enemigo a la meta
                        float dis = Math.Abs(enemies[i].CurrentPosition().GetPosition.x - goals[0].GetPosition.x) +
                                    Math.Abs(enemies[i].CurrentPosition().GetPosition.y - goals[0].GetPosition.y);
                        //Si dis > distancia actualizamos los valores de selectedEnemy, del buleano selected que ha sido seleccionado y 
                        //guardamos la distancia
                        if (dis > distance)
                        {
                            selectedEnemy = i;
                            selected = true;
                            distance = dis;
                        }
                    }
                }

            }

             
        }
    }
}
