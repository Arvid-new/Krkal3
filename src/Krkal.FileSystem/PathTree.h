
#pragma once

#ifndef PathTree_H
#define PathTree_H

#include <vector>
#include <string>
#include "types.h"
#include <algorithm>

template <typename T>
class CPathTree {
private:
	typedef std::vector<CPathTree*> childrenT;

	std::string _text; // text belonging to this node. Common prefix of child nodes. String ends before the first character that differs for childs.
	T * _content;
	childrenT _children;
	CPathTree* _parent;
	size_t _totalLen;


	bool Member(const std::string &key, CPathTree **retNode, size_t &pLen) {
		size_t len=0;
		while (pLen < key.length() && len < _text.length() && key[pLen] == _text[len]) len++, pLen++;

		char nextCh=0;
		if (pLen && pLen == key.length() && key[pLen-1] != '\\') {
			nextCh = '\\';
			if (len < _text.length()) {
				if (retNode)
					*retNode = this;
				return (len == _text.length()-1 && _text[len] == '\\'); // consider it equal also if the key misses the ending \ slash
			} 
		} else if (pLen < key.length()) {
			nextCh = key[pLen];
		}

		if (!nextCh || len < _text.length()) { // this node is the last one that matches the prefix
			if (retNode)
				*retNode = this;
			return (len == _text.length()); 
		}
	
		// search children
		for (childrenT::iterator i = _children.begin(); i != _children.end(); ++i) {
			if ((**i)._text[0] == nextCh) {
				return (**i).Member(key, retNode, pLen);
			}
		}

		// no child matches
		if (retNode)
			*retNode = this;
		return true;
	}


	CPathTree *Split(int len) {
		int len2 = len - (_totalLen - _text.length());
		CPathTree *ret = new CPathTree(_text.substr(0, len2), 0, _parent);
		if (_parent) {
			for (childrenT::iterator i = _parent->_children.begin(); i != _parent->_children.end(); ++i) {
				if (*i == this) {
					*i = ret;
					break;
				}
			}
		}

		ret->_children.push_back(this);
		_parent = ret;
		_text = _text.substr(len2, _totalLen - len);
		assert(!_text.empty());
		return ret;
	}

	CPathTree *AddChild(const std::string &text) {
		CPathTree *ret = new CPathTree(text, 0, this);
		_children.push_back(ret);
		return ret;
	}

	void JoinWithParent() {
		_text = _parent->_text + _text;
		CPathTree *np = _parent->_parent;
		if (np) {
			for (childrenT::iterator i = np->_children.begin(); i != np->_children.end(); ++i) {
				if (*i == _parent) {
					*i = this;
					break;
				}
			}
		}
		_parent = np;
	}

	void DeleteChild(CPathTree *child) {
		for (childrenT::iterator i = _children.begin(); i != _children.end(); ++i) {
			if (*i == child) {
				_children.erase(i);
				break;
			}
		}
		if (_content == 0)
			Delete();
	}

	CPathTree(const std::string &text, T*content, CPathTree* parent) {
		_text = text;
		assert(!_text.empty());
		_content = content;
		_parent = parent;
		_totalLen = _parent->_totalLen + _text.length();
	}
	~CPathTree() {
		SAFE_DELETE(_content);
	}

public:
	CPathTree() {
		_content = 0;
		_parent = 0;
		_totalLen = 0;
	}

	T *& GetContent() {
		return _content;
	}
	size_t GetTotalLen() {
		return _totalLen;
	}

	CPathTree *Find(const std::string &key) {
		CPathTree *ret;
		size_t len=0;
		if (Member(key, &ret, len) && len == key.length()) {
			return ret;
		} else {
			return 0;
		}
	}

	CPathTree *FindChildren(const std::string &key) {
		CPathTree *ret;
		size_t len=0;
		Member(key, &ret, len);
		if (len == key.length()) {
			return ret;
		} else {
			return 0;
		}
	}

	CPathTree *GetFirstPrefix(const std::string &key) {
		CPathTree *ret;
		size_t len=0;
		bool match = Member(key, &ret, len);
		if (!match || !ret->_content) {
			ret = ret->_parent;
			while (ret && !ret->_content)
				ret = ret->_parent;
		}

		return ret;
	}

	CPathTree *GetNextPrefix() {
		CPathTree *ret = _parent;
		while (ret && !ret->_content)
			ret = ret->_parent;
		return ret;
	}


	CPathTree *Insert(std::string key) {

		if (key.empty())
			return 0;

		if (key[key.length()-1] != '\\')
			key.push_back('\\');

		CPathTree *ret;
		size_t len=0;
		if (Member(key, &ret, len) && len == key.length())
			return ret;

		if (len != ret->_totalLen)
			ret = ret->Split(len);

		if (len == key.length())
			return ret;

		return ret->AddChild(key.substr(len));
	}

	CPathTree *GetRoot() {
		CPathTree *ret = this;
		while (ret->_parent) ret = ret->_parent;
		return ret;
	}

	void Delete() {
		SAFE_DELETE(_content);
		
		if (_parent) { // never delete the root!
			if (_children.size() == 1) {
				_children[0]->JoinWithParent();
				delete this;
			} else if (_children.size() == 0) {
				_parent->DeleteChild(this);
				delete this;
			}
		}
	}

	void DeleteAll() {
		for (childrenT::iterator i = _children.begin(); i != _children.end(); ++i) {
			(**i).DeleteAll();
		}
		_children.clear();
		delete this;
	}

	void DeleteChildren(bool andMe = false) {
		for (childrenT::iterator i = _children.begin(); i != _children.end(); ++i) {
			(**i).DeleteAll();
		}
		_children.clear();
		if (andMe || !_content)
			Delete();
	}


	size_t ChildrenCount() { return _children.size();}

	CPathTree *GetChild(size_t index) {
		return _children.at(index);
	}

	const std::string &GetText() { return _text; }

	std::string GetTotalText() {
		std::string ret(_totalLen, ' ');
		std::string::iterator i = ret.end();
		std::string::iterator sb, se, d;
		for (CPathTree *ptr = this; ptr; ptr = ptr->_parent) {
			i-=ptr->_text.size();
			d=i; sb = ptr->_text.begin(); se = ptr->_text.end();
			std::copy(sb, se, d);
		}
		return ret;
	}

};



#endif