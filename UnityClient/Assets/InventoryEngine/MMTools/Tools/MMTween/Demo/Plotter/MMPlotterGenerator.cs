using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace MoreMountains.Tools
{
    public class MMPlotterGenerator : MonoBehaviour
    {
        public MMPlotter PlotterPrefab;
        
        public Vector2 Spacing;
        public float VerticalOddSpacing;
        public int RowLength;

        [Header("Materials")]

        public Material LinearMaterial;
        public Material QuadraticMaterial;
        public Material CubicMaterial;
        public Material QuarticMaterial;
        public Material QuinticMaterial;
        public Material SinusoidalMaterial;
        public Material BounceMaterial;
        public Material OverheadMaterial;
        public Material ExponentialMaterial;
        public Material ElasticMaterial;
        public Material CircularMaterial;

        protected Vector2 _position;

        [MMInspectorButton("GeneratePlotters")]
        public bool GeneratePlottersButton;

        protected virtual void Awake()
        {
            Time.timeScale = 0f;

            GeneratePlotters();
        }

        protected virtual void GeneratePlotters()
        {
            this.transform.MMDestroyAllChildren();

            BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
            MethodInfo[] methods = typeof(MMTweenDefinitions).GetMethods(flags);

            int row = 0;
            int column = 0;
            float yCoordinate = 0;

            for (int i=0; i < methods.Length; i++)
            {
                _position.x = column * Spacing.x;
                

                _position.y = yCoordinate;
                
                MMPlotter plotter = Instantiate(PlotterPrefab);
                plotter.transform.SetParent(this.transform);
                plotter.transform.localPosition = _position;
                plotter.TweenMethodIndex = i;
                string tweenName = plotter.TweenName(plotter.TweenMethodIndex);
                plotter.gameObject.name = tweenName;

                Material newMaterial = LinearMaterial;
                if (tweenName.Contains("Linear")) { newMaterial = LinearMaterial; }
                if (tweenName.Contains("Quadratic")) { newMaterial = QuadraticMaterial; }
                if (tweenName.Contains("Cubic")) { newMaterial = CubicMaterial; }
                if (tweenName.Contains("Quartic")) { newMaterial = QuarticMaterial; }
                if (tweenName.Contains("Quintic")) { newMaterial = QuinticMaterial; }
                if (tweenName.Contains("Sinusoidal")) { newMaterial = SinusoidalMaterial; }
                if (tweenName.Contains("Bounce")) { newMaterial = BounceMaterial; }
                if (tweenName.Contains("Overhead")) { newMaterial = OverheadMaterial; }
                if (tweenName.Contains("Exponential")) { newMaterial = ExponentialMaterial; }
                if (tweenName.Contains("Elastic")) { newMaterial = ElasticMaterial; }
                if (tweenName.Contains("Circular")) { newMaterial = CircularMaterial; }

                plotter.SetMaterial(newMaterial);
                plotter.GetMethodsList();
                plotter.DrawGraph();

                if (column >= RowLength - 1)
                {
                    column = 0;                    
                    row++;
                    if (row % 2 == 0)
                    {
                        yCoordinate += Spacing.y + VerticalOddSpacing;
                    }
                    else
                    {
                        yCoordinate += Spacing.y;
                    }
                }
                else
                {
                    column++;
                }
            }
        }

    }
}
