using Assets.Scripts.DataStructures;
using System.Collections;
using UnityEngine;
//using Boo.Lang;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.Grupo
{
    public class Nodo
    {
       
        public Nodo NodoPadre { get; set; }
        public CellInfo current { get; set; }
        public float cost { get; set; }

        //Constructor clase Nodo
        public Nodo(CellInfo currentcell, CellInfo[] goal){
            this.NodoPadre = null;
            this.current = currentcell;
            //Calculamos distancia Manhattan para luego poder ordenar por coste los Nodos
            this.cost = currentcell.WalkCost + Mathf.Abs(currentcell.GetPosition.x - goal[0].GetPosition.x) 
                + Mathf.Abs(currentcell.GetPosition.y - goal[0].GetPosition.y);
        }

        
        //Función expandir del Nodo que le pasemos como parámetro y devolvemos una lista con los nodos expandidos.
        public List<Nodo> expand(Nodo camino, BoardInfo Board, CellInfo[] goal, List<Nodo> fathers) {
            //Lista de nodos expandidos
            List<Nodo> expandir = new List<Nodo>();
            //Array de nodos 
            Nodo[] nuevonodo = new Nodo[4];
            //array de tipo CellInfo donde guardaremos los vecinos atravesables cerca de él
            CellInfo[] array;
            array = camino.getCurrent().WalkableNeighbours(Board);

            for (int i = 0; i < 4; i++)
            {
                if (array[i] != null)
                {
                    if(array[i].Walkable)
                    {
                        
                        //Creamos un nuevo nodo de todas las posiciones del array de tipo cellinfo y le decimos que su padre
                        //es el nodo camino que introducimos como parámetro y lo añadimos a la lista expandir                    
                        nuevonodo[i] = new Nodo(array[i], goal);
                        nuevonodo[i].NodoPadre = camino;
                        expandir.Add(nuevonodo[i]);
                    }
                                     
                }
  
            }
            //Eliminamos ciclos simples con este paso para no volver atrás
            for (int i = 0; i < fathers.Count; i++)
            {
                for (int j = 0; j<expandir.Count; j++)
                {
                    if (fathers[i].current.GetPosition == expandir[j].current.GetPosition)
                    {
                        expandir.RemoveAt(j);
                    }
                }
                

            }
            //Ordenamos la lista de Nodos expandidos
            expandir = ordenar(expandir);

            return expandir;
        }

        //Ordenamos la lista de nodos expandidos antes de retornarla
        private List<Nodo> ordenar(List<Nodo> expandedNodos)
        {
            IList<Nodo> ord = expandedNodos;            
            expandedNodos = ord.OrderBy(x => x.cost).ToList();

            return expandedNodos;           
        }

        public CellInfo getCurrent()
        {
            return this.current;
        }

        public Nodo getFather()
        {
            return this.NodoPadre;
        }

        public void setFather(Nodo father)
        {
            NodoPadre = father;
        }

    } 
}

