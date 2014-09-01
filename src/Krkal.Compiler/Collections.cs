//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - C o l l e c t i o n s
///
///		Collections
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;

namespace Krkal.Compiler
{
	public enum GraphColor
	{
		White,
		Gray,
		Black,
	}




	public class Set<T> : KeyedCollection<T, T>, IList<T>, ICollection<T>, IList, ICollection
	{
		public Set() : base(null, 4) {}
		public Set(IEqualityComparer<T> comparer) : base(comparer, 4) { }
		public Set(IEqualityComparer<T> comparer, int dictionaryCreationThreshold) : base(comparer, dictionaryCreationThreshold) { }

		protected override T GetKeyForItem(T item) {
			return item;
		}

		public new void Add(T item) {
			if (!Contains(item)) {
				base.Add(item);
			}
		}
	}



}