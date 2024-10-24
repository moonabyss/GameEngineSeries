﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalEditor.Utilities
{
    interface IUndoRedo
    {
        string Name { get; }
        void Undo();
        void Redo();
    }

    class UndoRedoAction : IUndoRedo
    {
        private readonly Action _undoAction;
        private readonly Action _redoAction;
        public string Name { get; }

        public void Undo() => _undoAction();
        public void Redo() => _redoAction();

        public UndoRedoAction(string name) { Name = name; }

        public UndoRedoAction(Action undo, Action redo, string name)
            : this(name)
        {
            _undoAction = undo;
            _redoAction = redo;
        }

        public UndoRedoAction(string property, object instance, object undoValue, object redoValue, string name)
            : this(
                  () => instance.GetType().GetProperty(property).SetValue(instance, undoValue),  //
                  () => instance.GetType().GetProperty(property).SetValue(instance, redoValue),  //
                  name
              )
        {}
    }

    class UndoRedo
    {
        private bool _enableAdd = true;
        private readonly ObservableCollection<IUndoRedo> _redoList = [];
        private readonly ObservableCollection<IUndoRedo> _undoList = [];
        public ReadOnlyObservableCollection<IUndoRedo> RedoList { get; }
        public ReadOnlyObservableCollection<IUndoRedo> UndoList { get; }

        public void Reset()
        {
            _redoList.Clear();
            _undoList.Clear();
        }

        public void Add(IUndoRedo cmd)
        {
            if (_enableAdd)
            {
                _undoList.Add(cmd);
                _redoList.Clear();
            }
        }

        public void Undo()
        {
            if (_undoList.Any())
            {
                var cmd = _undoList.Last();
                _undoList.RemoveAt(_undoList.Count - 1);
                _enableAdd = false;
                cmd.Undo();
                _enableAdd = true;
                _redoList.Insert(0, cmd);
            }
        }

        public void Redo()
        {
            if (_redoList.Any())
            {
                var cmd = _redoList.First();
                _redoList.RemoveAt(0);
                _enableAdd = false;
                cmd.Redo();
                _enableAdd = true;
                _undoList.Add(cmd);
            }
        }

        public UndoRedo()
        {
            RedoList = new ReadOnlyObservableCollection<IUndoRedo>(_redoList);
            UndoList = new ReadOnlyObservableCollection<IUndoRedo>(_undoList);
        }
    }
}
