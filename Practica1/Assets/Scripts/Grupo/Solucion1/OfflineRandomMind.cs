using Assets.Scripts.DataStructures;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Assets.Scripts.Grupo
{
    public class OfflineRandomMind : AbstractPathMind
    {
        public bool isActive;
        bool activated;
        int contador;
        public Animator anim;
        List<Nodo> fathers;
        List<Nodo> path;
        bool encontrado;

        private void Awake()
        {
            /*
            * Le decimos al editor que ponga en primer lugar este script en caso de que el boolean isActive == true
            * así podemos tener los dos scripts juntos y ejecutar el que nosotros queramos activando la casilla isActive en el script
            * que queramos
            */
            if (isActive)
            {                          
                UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
                //Escondemos el canvas porque en el método offline no lo necesitamos
                var canvas = FindObjectOfType<Canvas>();
                canvas.gameObject.SetActive(false);
            }
            else
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(this);
                enabled = false;
            }
            activated = false;
            contador = 0;
            fathers = new List<Nodo>();
            path = new List<Nodo>();
            encontrado = false;
        }

        public override void Repath()
        {

        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            //con este if lo que hacemos es activar dontDestroyOnload que hemos deshabilitado en gameManager para poder recargar el nivel
            //en el script QLearningRandomMind
            if (!activated)
            {
                //Lo activamos con el método ActiveDontDestroyOnLoad en la clase GameManager y ponemos el boolean activated a true para
                //que no vuelva a entrar aquí
                var loader = FindObjectOfType<Loader>();
                loader.manager.ActiveDontDestroyOnLoad();
                activated = true;
            }

            //Si no ha calculado la ruta a la meta entra en el if 
            if (!encontrado)
            {
                //LLama al método calculatePath2 que calcula la ruta a recorrer hasta la meta
                calculatePath2(currentPos, goals, boardInfo);             
            }
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
            //Hemos llegado a la meta
            else
            {
                 futurePosX = goals[0].GetPosition.x;
                 futurePosY = fathers[0].current.GetPosition.y;
                
            }
            /*
             * Activamos las animaciones del pesonaje según la dirección en la que nos movamos y borramos la primera posición de 
             * la lista de padres (fathers) hasta que lleguemos a la meta.
             */
            if (currentPosX > futurePosX && currentPosY == futurePosY)
                {
                    anim.SetFloat("velocityX", -1f);
                    anim.SetFloat("velocityY", 0f);
                    fathers.RemoveAt(0);
                    return Locomotion.MoveDirection.Left;
                }
                else if (currentPosX < futurePosX && currentPosY == futurePosY)
                {
                    anim.SetFloat("velocityX", 1f);
                    anim.SetFloat("velocityY", 0f);
                    fathers.RemoveAt(0);
                    return Locomotion.MoveDirection.Right;
                }
                else if (currentPosY > futurePosY)
                {
                    anim.SetFloat("velocityY", -1f);
                    anim.SetFloat("velocityX", 0f);
                    fathers.RemoveAt(0);
                    return Locomotion.MoveDirection.Down;
                }
                else if (currentPosY < futurePosY)
                {
                    anim.SetFloat("velocityY", 1f);
                    anim.SetFloat("velocityX", 0f);
                    fathers.RemoveAt(0);
                    return Locomotion.MoveDirection.Up;
                }
                else
                {
                    anim.SetFloat("velocityX", 0f);
                    anim.SetFloat("velocityY", 0f);
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
                if(fathers[fathers.Count - 1].current.Equals(goals[0]))
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
                        if(nodoPadre != null)
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
                       Debug.Log("No hay camino y tengo que coger abiertos Padre. Coste: " + fathers[fathers.Count-1].cost + "Posición: " + fathers[fathers.Count - 1].current.GetPosition.ToString());
                       Debug.Log("No hay camino y tengo que coger abiertos. Coste: "+ abiertos[0].cost+ "Posición: " + abiertos[0].current.GetPosition.ToString());

                        //Si expandednodos está vacía pasamos el primero de la lista de abiertos y limpiamos expandedNodos y expandimos otra vez
                        expandedNodos.Clear();
                        if(abiertos.Count > 0)
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
            
    }
}
