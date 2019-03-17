using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Assets.Scripts.DataStructures;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;

namespace Assets.Scripts.Grupo
{
    public class QLearningRandomMind : AbstractPathMind
    {
        public Animator anim;
        public bool isActive;
        int seed;
        bool initialized;
        bool loaded ;
        QLearning aprender;
        private int contador;

        //Añadimos text para mostrar epsilon
        public Text Epsilontext;

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
                var canvas = FindObjectOfType<Canvas>();
                canvas.gameObject.SetActive(true);
                //Velocidad de ejecución
                Time.timeScale = 3;
            }
            else
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(this);
                enabled = false;
            }
            //Inicializamos variables
            seed = FindObjectOfType<Loader>().seed;
            initialized = false;
            loaded = false;
            contador = 0;

        }

        public override void Repath()
        {

        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            //Si el archivo existe lo cargamos y cambiamos la variable loaded a true y mostramos el epsilon actual
            if (File.Exists(Application.dataPath + "/Scripts/Grupo/Solucion3/QLearningInfo" + seed + ".dat"))
            {
                if (!loaded)
                {
                    load();
                    loaded = !loaded;
                    Epsilontext.text = "Epsilon: " + Mathf.Round(aprender.getE()*10000)/10000;
                }
                
            }
            //Si no existe el archivo creamos el objeto y lo guardamos, cambiamos los buleanos necesarios a true y mostramos epsilon
            else
            {
                if (!initialized)
                {
                    aprender = new QLearning(boardInfo, seed);
                    initialized = true;
                    loaded = !loaded;
                    Epsilontext.text = "Epsilon: " + aprender.getE().ToString();
                }
            }

            /*
             * invocamos el método nextStep del objeto creado aprender que nos devuelve el siguiente paso a realizar
             */
            int next = aprender.nextStep(currentPos, boardInfo);

            if (next == 0)
            {
                //Si la posición a la que nos vamos a mover es la meta Guardamos todos los valores y recargamos la escena
                Vector2 pos = new Vector2(currentPos.GetPosition.x, currentPos.GetPosition.y + 1);
                if(pos == goals[0].GetPosition || aprender.maxIterations <= 0)
                {
                    save();
                    reloadGame();
                }
                //Reducimos el máximo de iteraciones
                aprender.maxIterations--;
                //activamos animaciones
                anim.SetFloat("velocityY", 1f);
                anim.SetFloat("velocityX", 0f);
                return Locomotion.MoveDirection.Up;
            }
            else if (next == 1)
            {
                //Si la posición a la que nos vamos a mover es la meta Guardamos todos los valores y recargamos la escena
                Vector2 pos = new Vector2(currentPos.GetPosition.x, currentPos.GetPosition.y - 1);
                if (pos == goals[0].GetPosition || aprender.maxIterations <= 0)
                {
                    save();
                    reloadGame();
                }
                //Reducimos el máximo de iteraciones
                aprender.maxIterations--;
                //activamos animaciones
                anim.SetFloat("velocityY", -1f);
                anim.SetFloat("velocityX", 0f);
                return Locomotion.MoveDirection.Down;
            }
            else if (next == 2)
            {
                //Si la posición a la que nos vamos a mover es la meta Guardamos todos los valores y recargamos la escena
                Vector2 pos = new Vector2(currentPos.GetPosition.x - 1, currentPos.GetPosition.y);
                if (pos == goals[0].GetPosition || aprender.maxIterations <= 0)
                {
                    save();
                    reloadGame();
                }
                //Reducimos el máximo de iteraciones
                aprender.maxIterations--;
                //activamos animaciones
                anim.SetFloat("velocityX", -1f);
                anim.SetFloat("velocityY", 0f);
                return Locomotion.MoveDirection.Left;
            }
            else if (next == 3)
            {
                //Si la posición a la que nos vamos a mover es la meta Guardamos todos los valores y recargamos la escena
                Vector2 pos = new Vector2(currentPos.GetPosition.x + 1, currentPos.GetPosition.y);
                if (pos == goals[0].GetPosition || aprender.maxIterations <= 0)
                {
                    save();
                    reloadGame();
                }
                //Reducimos el máximo de iteraciones
                aprender.maxIterations--;
                //activamos animaciones
                anim.SetFloat("velocityX", 1f);
                anim.SetFloat("velocityY", 0f);
                return Locomotion.MoveDirection.Right;
            }
            else
            {
                //Actualizamos las animaciones para que no haga nada
                anim.SetFloat("velocityX", 0f);
                anim.SetFloat("velocityY", 0f);
                return Locomotion.MoveDirection.None;
            }
        }
        //Guardamos la información en un archivo dentro de la carpeta Grupo/Solucion3
        //Tendrá guardados todos los archivos con todas las semillas probadas
        void save()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file;

            if (File.Exists(Application.dataPath + "/Scripts/Grupo/Solucion3/QLearningInfo" + seed + ".dat"))
            {              
                 file = File.Open(Application.dataPath + "/Scripts/Grupo/Solucion3/QLearningInfo" + seed + ".dat", FileMode.Open);
                 //Debug.Log("Open");
            }
            else
            {
                 file = File.Create(Application.dataPath + "/Scripts/Grupo/Solucion3/QLearningInfo" + seed + ".dat");
                Debug.Log("Created");
            }
            //Le decimos que guarde el objeto aprender
            bf.Serialize(file, aprender);
            //Cerramos el archivo
            file.Close();
        }
        //En caso de que el script exista lo cargamos
        void load()
        {
            //Si el archivo existe lo cargamos y actualizamos los valores del maximo de iteraciones a 600 y reducimos epsilon
            if(File.Exists(Application.dataPath + "/Scripts/Grupo/Solucion3/QLearningInfo" + seed + ".dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.OpenRead(Application.dataPath + "/Scripts/Grupo/Solucion3/QLearningInfo" + seed + ".dat");

                aprender = (QLearning)bf.Deserialize(file);
                file.Close();             
                aprender.setE(0.05f);
                aprender.maxIterations = 600;
                //Si queremos ver los valores de la tabla QTable podemos activar esta opción
                //aprender.printTable();
                Debug.Log("Loaded");
            }

        }
        //Borramos el archivo
        void deleteFile()
        {
            File.Delete(Application.dataPath + "/Scripts/Grupo/Solucion3/QLearningInfo" + seed + ".dat");
        }
        //Recargamos el juego en caso de que lleguemos a la meta
        void reloadGame()
        {
            SceneManager.LoadScene("PathFinding");
        }
        //Si presionamos el botón ExitButton salimos de la aplicación
        public void exitQlearning()
        {
            EditorApplication.isPlaying = false;
            Application.Quit();
        }
    }

}