using DotNext.Native;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SharedMemory {
	class MainViewModel : BindableBase, IDisposable {
		readonly ObservableCollection<string> _log = new ObservableCollection<string>();
		MemoryMappedFile _memFile;
		PrivateObjectNamespace _objectNamespace;

		public IList<string> Log => _log;

		public DelegateCommandBase WriteCommand { get; }
		public DelegateCommandBase ReadCommand { get; }
		public DelegateCommandBase CreateCommand { get; }
		public DelegateCommandBase CloseCommand { get; }

		const string SharedMemName = "MySharedMem";
		const string BoundaryName = "MyBoundary";
		const string PrivateNamespaceName = "MyPrivateNamespace";

		private bool _usePrivateNamespace;

		public bool UsePrivateNamespace {
			get => _usePrivateNamespace;
			set => SetProperty(ref _usePrivateNamespace, value);
		}

		string _text;
		public string Text {
			get => _text;
			set => SetProperty(ref _text, value);
		}

		public MainViewModel() {
			WriteCommand = new DelegateCommand(() => {
				using (var stm = _memFile.CreateViewStream()) {
					using (var writer = new StreamWriter(stm)) {
						writer.WriteLine(Text);
					}
				}
				Log.Add($"Writing: {Text}");
			}, () => _memFile != null);

			ReadCommand = new DelegateCommand(() => {
				using (var stm = _memFile.CreateViewStream()) {
					using (var reader = new StreamReader(stm)) {
						if (reader.Peek() == 0)
							Text = string.Empty;
						else
							Text = reader.ReadLine();
					}
					Log.Add($"Reading: {Text}");
				}
			}, () => _memFile != null);

			CreateCommand = new DelegateCommand(() => {
				try {
					if (!UsePrivateNamespace) {
						_memFile = MemoryMappedFile.CreateOrOpen(SharedMemName, 1 << 20);
						Log.Add("Create/open section (no private namespace)");
					}
					else {
						_objectNamespace?.Dispose();
						CreatePrivateNamespace();
						_memFile = MemoryMappedFile.CreateOrOpen(PrivateNamespaceName + "\\" + SharedMemName, 1 << 20);
						Log.Add("Create/open section with private namespace");
					}
					UpdateState();
				}
				catch (Exception ex) {
					MessageBox.Show(ex.Message);
				}
			}, () => _memFile == null);

			CloseCommand = new DelegateCommand(() => {
				_memFile?.Dispose();
				_memFile = null;
				UpdateState();
				Text = string.Empty;
				Log.Add("Closed MMF");
			}, () => _memFile != null);
		}

		private void UpdateState() {
			CloseCommand.RaiseCanExecuteChanged();
			CreateCommand.RaiseCanExecuteChanged();
			WriteCommand.RaiseCanExecuteChanged();
			ReadCommand.RaiseCanExecuteChanged();
		}

		private void CreatePrivateNamespace() {
			_objectNamespace = new PrivateObjectNamespace(BoundaryName, PrivateNamespaceName);
		}

		public void Dispose() {
			_memFile?.Dispose();
			_objectNamespace?.Dispose();
		}
	}
}
