﻿/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Undo.Records
{
    using Elements;

    /// <summary>
    /// Record that undoes adding an scenario element to the map
    /// </summary>
    public class UndoAddElement : UndoRecord
    {
        /// <summary>
        /// Scenario element that was added to the map
        /// </summary>
        private readonly ScenarioElement scenarioElement;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scenarioElement">Scenario element that was added to the map</param>
        public UndoAddElement(ScenarioElement scenarioElement)
        {
            this.scenarioElement = scenarioElement;
        }
        
        /// <inheritdoc/>
        public override void Undo()
        {
            scenarioElement.RemoveFromMap();
            scenarioElement.Dispose();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            
        }
    }
}