//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.CodeGenerator - M e t h o d W r i t e r
///
///		Generates output for a method body
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;
using Krkal.Compiler;
using System.Globalization;

namespace Krkal.CodeGenerator
{
	internal class MethodWriter
	{

		StringBuilder _sb = new StringBuilder();
		KSWriter _ksWriter;
		int _indent;
		MethodField _methodField;
		ClassName _thisClassName;
		int _tempCounter;

		// CONSTRUCTOR
		public MethodWriter(KSWriter ksWriter, MethodField methodField) {
			_ksWriter = ksWriter;
			_methodField = methodField;
			_thisClassName = methodField.InheritedFrom;
		}

		public MethodWriter(KSWriter ksWriter, ClassName className) {
			_ksWriter = ksWriter;
			_thisClassName = className;
		}





		public String GenerateContent(MethodAnalysis methodAnalysis) {
			if (methodAnalysis.CodeBlock != null) {
				DoBlockContent(methodAnalysis.CodeBlock);
			} else {
				DoExternMethod(methodAnalysis);
			}
			return _sb.ToString();
		}


		public String GenerateInitializationContent() {
			foreach (UniqueField field in _thisClassName.UniqueNames.Values) {
				if (field.Assignment != null) {
					_sb.Append('\t');

					LanguageType lt = ((TypedKsidName)field.Name).LanguageType.ClearModifier();

					if (CompilerConstants.IsNameTypeStatic(field.Name.NameType)) {
						_sb.Append("(*Script->");
						_sb.Append(_ksWriter.KSNames.GetStaticVariable(field.Name));
						_sb.Append(')');
					} else {
						_sb.Append("thisO.get<");
						_sb.Append(Utils.ParseType(lt));
						_sb.Append(">(Script->");
						_sb.Append(_ksWriter.KSNames.GetMemberVariable(_thisClassName, field.Name));
						_sb.Append(')');
					}

					_sb.Append(" = ");
					DoExpression(field.Assignment, lt);
					_sb.Append(';');

					_sb.AppendLine();
				}
			}
			return _sb.ToString();
		}





		private void DoExternMethod(MethodAnalysis methodAnalysis) {
			int index = 0;
			foreach (ExternMethodSubstitution substitution in methodAnalysis.SubstitutedNames) {
				_sb.Append(methodAnalysis.ExternBlock.Substring(index, substitution.StartIndex - index));
				index = substitution.EndIndex;
				_sb.Append("Script->");

				switch (substitution.Type) {
					case ExternMethodSubstitutionType.Ksid:
						_sb.Append(_ksWriter.KSNames.GetKsidName(substitution.Name1));
						break;
					case ExternMethodSubstitutionType.StaticVariable:
						_sb.Append(_ksWriter.KSNames.GetStaticVariable(substitution.Name1));
						break;
					case ExternMethodSubstitutionType.MemberVariable:
						_sb.Append(_ksWriter.KSNames.GetMemberVariable(substitution.Name1, substitution.Name2));
						break;
					case ExternMethodSubstitutionType.Cast:
						_sb.Append(_ksWriter.KSNames.GetCast(substitution.Name1, substitution.Name2));
						break;
					case ExternMethodSubstitutionType.DirectMethod:
						_sb.Append(_ksWriter.KSNames.GetDirectMethod(substitution.Name1));
						break;
					default:
						throw new InternalCompilerException();
				}
			}
			_sb.Append(methodAnalysis.ExternBlock.Substring(index, methodAnalysis.ExternBlock.Length - index));
		}






		private PositionInLines DoBlockContent(CodeBlock codeBlock) {
			_indent++;
			PositionInLines prevStatement = codeBlock.FirstToken.PositionInLines;
			foreach (Statement statement in codeBlock.Statements) {
				Indent(prevStatement, statement.PositionInLines);
				DoStatement(statement);
				prevStatement = statement.PositionInLines;
			}
			_indent--;
			return prevStatement;
		}





		private void Indent(PositionInLines prevStatement, PositionInLines nextStatement) {
			if (prevStatement.LastRow >= nextStatement.FirstRow) {
				WriteSpace();
			} else {
				for (int f=0; f < nextStatement.FirstRow - prevStatement.LastRow; f++)
					_sb.AppendLine();
				for (int f = 0; f < _indent; f++)
					_sb.Append('\t');
			}
		}



		private void IndentRight(PositionInLines positionInLines, Statement statement) {
			if (statement.GetStatementType() != StatementType.Block)
				_indent++;
			Indent(positionInLines, statement.PositionInLines);
		}


		private void IndentLeft(Statement statement) {
			if (statement.GetStatementType() != StatementType.Block)
				_indent--;
		}

		private void WriteLine(int newIndent) {
			_sb.AppendLine();
			_indent = newIndent;
			for (int f = 0; f < _indent; f++)
				_sb.Append('\t');
		}







		private void DoStatement(Statement statement) {

			switch (statement.GetStatementType()) {

				case StatementType.Block:
					_sb.Append('{');
					PositionInLines prevStatement = DoBlockContent(statement as CodeBlock);
					Indent(prevStatement, statement.LastToken.PositionInLines);
					_sb.Append('}');
					return;

				case StatementType.Declaration:
					DoDeclaration(statement as LocalDeclaration);
					return;

				case StatementType.Expression:
					DoExpression(statement as Expression);
					_sb.Append(';');
					return;

				case StatementType.Command:
					DoCommand(statement as Commanand);
					return;

				default:
					throw new InternalCompilerException();

			}
		}





		private void DoCommand(Commanand commanand) {
			switch (commanand.Type) {
				case CommandType.Break:
					_sb.Append("break;");
					return;
				case CommandType.Continue:
					_sb.Append("continue;");
					return;
				case CommandType.Do:
					DoDoCommand(commanand as DoCommanand);
					return;
				case CommandType.For:
					DoForCommand(commanand as ForCommanand);
					return;
				case CommandType.ForEach:
					DoForEachCommand(commanand as ForEachCommanand);
					return;
				case CommandType.If:
					DoIfCommand(commanand as IfCommanand);
					return;
				case CommandType.Return:
					DoReturnCommand(commanand as ReturnCommanand);
					return;
				case CommandType.While:
					DoWhileCommand(commanand as WhileCommanand);
					return;
				default:
					throw new InternalCompilerException();

			}
		}



		private void DoWhileCommand(WhileCommanand whileCommanand) {
			_sb.Append("while (");
			DoExpression(whileCommanand.Condition);
			_sb.Append(')');
			DoCommanandBody(whileCommanand.Condition.PositionInLines, whileCommanand.Body);
		}



		private PositionInLines DoCommanandBody(PositionInLines positionInLines, Statement statement) {
			if (statement == null) {
				_sb.Append(" ;");
				return positionInLines;
			} else {
				IndentRight(positionInLines, statement);
				DoStatement(statement);
				IndentLeft(statement);
				return statement.PositionInLines;
			}
		}



		private void DoReturnCommand(ReturnCommanand returnCommanand) {
			if (returnCommanand.Expression != null) {
				if (CompilerConstants.IsNameTypeDirectMethod(_methodField.Name.NameType)) {
					_sb.Append("return ");
					DoExpression(returnCommanand.Expression, _methodField.Name.LanguageType);
					_sb.Append(";");
				} else {
					_sb.Append("{ ctx.ret<");
					_sb.Append(Utils.ParseType(_methodField.Name.LanguageType));
					_sb.Append(">() = ");
					DoExpression(returnCommanand.Expression, _methodField.Name.LanguageType);
					_sb.Append("; return; }");
				}
			} else {
				_sb.Append("return;");
			}
		}



		private void DoIfCommand(IfCommanand ifCommanand) {
			_sb.Append("if (");
			DoExpression(ifCommanand.Condition);
			_sb.Append(')');

			PositionInLines lastPos = DoCommanandBody(ifCommanand.Condition.PositionInLines, ifCommanand.IfPart);
					
			if (ifCommanand.ElsePart != null) {
				Indent(lastPos, ifCommanand.ElseToken.PositionInLines);
				_sb.Append("else");
				if (ifCommanand.ElsePart as IfCommanand != null) {
					Indent(ifCommanand.ElseToken.PositionInLines, ifCommanand.ElsePart.PositionInLines);
					DoStatement(ifCommanand.ElsePart);
				} else {
					DoCommanandBody(ifCommanand.ElseToken.PositionInLines, ifCommanand.ElsePart);
				}
			}
		}



		private void DoForEachCommand(ForEachCommanand forEachCommanand) {
			_sb.Append('{');
			WriteLine(_indent+1);

			LanguageType arrayType = forEachCommanand.Array.RootNode.LanguageType;
			LanguageType iteratorType = forEachCommanand.Declaration.LanguageType;

			_sb.Append(Utils.ParseType(arrayType));
			WriteSpace();
			int array = _tempCounter++;
			WriteTempVariable(array);
			_sb.Append(" = ");
			DoExprNode(forEachCommanand.Array.RootNode);
			_sb.Append(';');

			WriteLine(_indent);

			int dimensions = arrayType.DimensionsCount - iteratorType.DimensionsCount;
			int iterators = _tempCounter;
			_tempCounter += dimensions;

			for (int dim = 0; dim < dimensions; dim++) {
				_sb.AppendFormat(CultureInfo.InvariantCulture, "for(int _KST_{0} = 0; _KST_{0} < {1}->GetCount(); _KST_{0}++) ", iterators+dim, GetTempArray(array, dim));
			}

			_sb.Append('{');
			WriteLine(_indent+1);

			_sb.Append(Utils.ParseType(iteratorType));
			WriteSpace();
			WriteVariableName(forEachCommanand.Declaration.Name);
			_sb.Append(" = ");
			arrayType.DimensionsCount -= (byte)dimensions;
			DoCast(iteratorType, arrayType, forEachCommanand.Array.Cast, GetTempArray(array, dimensions));
			_sb.Append(';');

			if (forEachCommanand.Condition != null) {
				WriteLine(_indent);
				_sb.Append("if (");
				DoExpression(forEachCommanand.Condition);
				_sb.Append(")");
			}

			DoCommanandBody(forEachCommanand.Array.PositionInLines, forEachCommanand.Body);

			WriteLine(_indent - 1);
			_sb.Append('}');
			WriteLine(_indent - 1);
			_sb.Append('}');

		}


		private static String GetTempArray(int array, int deindex) {
			StringBuilder sb = new StringBuilder();
			sb.Append("_KST_");
			sb.Append(array);
			for (int f = 0; f < deindex; f++) {
				sb.Append("[_KST_");
				sb.Append(array + f + 1);
				sb.Append(']');
			}
			return sb.ToString();
		}




		private void DoForCommand(ForCommanand forCommanand) {
			_sb.Append("for (");
			if (forCommanand.Initialization != null) {
				DoStatement(forCommanand.Initialization);
			} else {
				_sb.Append(';');
			}
			WriteSpace();
			if (forCommanand.Condition != null) {
				DoExpression(forCommanand.Condition);
			}
			_sb.Append("; ");
			if (forCommanand.LastStep != null) {
				DoExpression(forCommanand.LastStep);
			}
			_sb.Append(')');

			DoCommanandBody(forCommanand.Condition.PositionInLines, forCommanand.Body);
		}



		private void DoDoCommand(DoCommanand doCommanand) {
			_sb.Append("do");
			PositionInLines lastPos = DoCommanandBody(doCommanand.FirstToken.PositionInLines, doCommanand.Body);
			Indent(lastPos, doCommanand.Condition.PositionInLines);
			_sb.Append("while (");
			DoExpression(doCommanand.Condition);
			_sb.Append(");");
		}







		private void DoExpression(Expression expression) {
			DoExprNode(expression.RootNode);
		}

		private void DoExpression(Expression expression, LanguageType expectedType) {
			DoCast(expectedType, expression.Cast, expression.RootNode);
		}



		private void DoCast(LanguageType expectedType, OperatorArgumentCasts cast, ExprNode node) {
			if (expectedType != node.LanguageType && !node.LanguageType.IsNull) {
				switch (cast) {
					case OperatorArgumentCasts.CastArithmeticsToResult:
						if (expectedType.BasicType != BasicType.Double && node.LanguageType.BasicType == BasicType.Double) {
							_sb.Append("Round(");
							DoExprNode(node);
							_sb.Append(")");
							return;
						}
						break;
					case OperatorArgumentCasts.CastObjectToBase:
						if (expectedType.ObjectType != null) {
							DoExprNode(node);
							_sb.Append(".Cast(Script->");
							_sb.Append(_ksWriter.KSNames.GetCast(node.LanguageType.ObjectType, expectedType.ObjectType));
							_sb.Append(')');
							return;
						}
						break;
					case OperatorArgumentCasts.CastObjectToDerived:
						DoExprNode(node);
						_sb.Append(".Cast(");
						WriteKsidName(expectedType.ObjectType);
						_sb.Append(')');
						return;
				}
			}

			DoExprNode(node);
		}


		private void DoCast(LanguageType expectedType, LanguageType type, OperatorArgumentCasts cast, String node) {
			if (expectedType != type && !type.IsNull) {
				switch (cast) {
					case OperatorArgumentCasts.CastArithmeticsToResult:
						if (expectedType.BasicType != BasicType.Double && type.BasicType == BasicType.Double) {
							_sb.Append("Round(");
							_sb.Append(node);
							_sb.Append(")");
							return;
						}
						break;
					case OperatorArgumentCasts.CastObjectToBase:
						if (expectedType.ObjectType != null) {
							_sb.Append(node);
							_sb.Append(".Cast(Script->");
							_sb.Append(_ksWriter.KSNames.GetCast(type.ObjectType, expectedType.ObjectType));
							_sb.Append(')');
							return;
						}
						break;
					case OperatorArgumentCasts.CastObjectToDerived:
						_sb.Append(node);
						_sb.Append(".Cast(");
						WriteKsidName(expectedType.ObjectType);
						_sb.Append(')');
						return;
				}
			}

			_sb.Append(node);
		}



		private void DoExprNode(ExprNode node) {
			DoExprNode(node, false);
		}

		private void DoExprNode(ExprNode node, bool lockArray) {
			if (node.WasInParenthesis)
				_sb.Append('(');

			switch (node.GetNodeType()) {
				case ExprNodeType.Array:
					DoArrayConstruction(node as ExprArray);
					break;
				case ExprNodeType.ArrayAccess:
					DoArrayAccess(node as ExprArrayAccess, lockArray);
					break;
				case ExprNodeType.Binary:
					DoBinary(node as ExprBinary);
					break;
				case ExprNodeType.Constant:
					DoConstant(node as ExprConstant);
					break;
				case ExprNodeType.DirectCall:
					DoDirectCall(node as ExprDirectCall);
					break;
				case ExprNodeType.KsidName:
					DoKsidName(node as ExprKsidName);
					break;
				case ExprNodeType.LocalVariable:
					DoLocalVariable(node as ExprLocalVariable);
					break;
				case ExprNodeType.ObjectAccess:
					DoObjectAccess(node as ExprObjectAccess);
					break;
				case ExprNodeType.SafeCall:
					DoSafeCall(node as ExprSafeCall);
					break;
				case ExprNodeType.StaticAccess:
					DoStaticAccess(node as ExprStaticAccess);
					break;
				case ExprNodeType.SystemMethod:
					DoSystemMethod(node as ExprSystemMethod);
					break;
				case ExprNodeType.Unary:
					DoUnary(node as ExprUnary);
					break;
				case ExprNodeType.Assigned:
					DoAssigned(node as ExprAssigned);
					break;
				default:
					throw new InternalCompilerException();
			}

			if (node.WasInParenthesis)
				_sb.Append(')');

		}








		private void DoAssigned(ExprAssigned exprAssigned) {
			_sb.Append("ctx.assigned(");
			_sb.Append(exprAssigned.Parameter.Index);
			_sb.Append(')');
		}

		

		private void DoUnary(ExprUnary exprUnary) {
			if (exprUnary.PostFix) {
				DoExprNode(exprUnary.Child);
				_sb.Append(exprUnary.Token.Operator.Text);
			} else {
				_sb.Append(exprUnary.Token.Operator.Text);
				if (exprUnary.Child is ExprUnary)
					WriteSpace();
				DoExprNode(exprUnary.Child);
			}
		}



		private void DoSystemMethod(ExprSystemMethod exprSystemMethod) {
			bool appendComma = false;

			SystemMethodTag tag = exprSystemMethod.SystemMethod.Tag != null ? (SystemMethodTag)exprSystemMethod.SystemMethod.Tag : SystemMethodTag.None;


			if (tag != SystemMethodTag.Clone && (exprSystemMethod.Obj.LanguageType.DimensionsCount > 0 || exprSystemMethod.Obj.LanguageType.BasicType == BasicType.Name)) {
				DoExprNode(exprSystemMethod.Obj);
				_sb.Append("->");
				_sb.Append(exprSystemMethod.SystemMethod.Name);
				_sb.Append('(');
			} else if (tag == SystemMethodTag.Clone || exprSystemMethod.Obj.LanguageType.BasicType == BasicType.Object) {
				DoExprNode(exprSystemMethod.Obj);
				_sb.Append(".");
				_sb.Append(exprSystemMethod.SystemMethod.Name);
				_sb.Append('(');
			} else {
				_sb.Append(exprSystemMethod.SystemMethod.Name);
				_sb.Append('(');
				DoExprNode(exprSystemMethod.Obj);
				appendComma = true;
			}


			if (tag == SystemMethodTag.AddCodeLineAndKerMain || (tag == SystemMethodTag.Clone && exprSystemMethod.Obj.LanguageType.DimensionsCount == 0)) {
				if (appendComma)
					_sb.Append(", ");
				_sb.Append(exprSystemMethod.Token.PositionInLines.FirstRow);
				_sb.Append(", KerMain");
				appendComma = true;
			}

			for (int f = 0; f < exprSystemMethod.Parameters.Count; f++) {
				if (appendComma) {
					_sb.Append(", ");
				} else {
					appendComma = true;
				}
				WriteDirectParam(exprSystemMethod.ParameterTypes[f], exprSystemMethod.ParametersCasts[f], exprSystemMethod.Parameters[f]);
			}

			_sb.Append(')');
		}



		private void WriteDirectParam(LanguageType type, OperatorArgumentCasts cast, ExprNode exprNode) {
			if ((type.Modifier & Modifier.Ret) != 0) {
				DoExprNode(exprNode, true);
			} else {
				DoCast(type, cast, exprNode);
			}
		}



		private void DoStaticAccess(ExprStaticAccess exprStaticAccess) {
			_sb.Append("(*Script->");
			_sb.Append(_ksWriter.KSNames.GetStaticVariable(exprStaticAccess.VariableName));
			_sb.Append(')');
		}



		private void DoSafeCall(ExprSafeCall exprSafeCall) {
			if (exprSafeCall.MessageType == MessageType.None) {
				// call

				if (exprSafeCall.LanguageType.IsVoid) {
					_sb.Append("KerMain->call(");
					WriteSafeCallParams(exprSafeCall);
				} else {
					_sb.Append("KerMain->call<");
					_sb.Append(Utils.ParseType(exprSafeCall.LanguageType));
					_sb.Append(">(");
					WriteSafeCallParams(exprSafeCall);
					WriteTypeParam(exprSafeCall.LanguageType);
				}
				_sb.Append(", ");
				_sb.Append(exprSafeCall.Parameters.Count);

				for (int f = 0; f < exprSafeCall.Parameters.Count; f++) {
					WriteTypeParam(exprSafeCall.Parameters[f].LanguageType);
					WriteSafeParam(exprSafeCall.ParamNames[f]);
					WriteSafeParam(exprSafeCall.Parameters[f]);
				}

			} else {
				// message

				_sb.Append("KerMain->message(");
				WriteSafeCallParams(exprSafeCall);
				_sb.Append(", ");
				_sb.Append((int)exprSafeCall.MessageType);
				_sb.Append(", ");

				if (exprSafeCall.MessageType == MessageType.Timed) {
					DoCast(new LanguageType(BasicType.Int), exprSafeCall.MeassageArgumentCast, exprSafeCall.MeassageArgument);
					_sb.Append(", 0, ");
				} else if (exprSafeCall.MessageType == MessageType.CallEnd) {
					_sb.Append("0, ");
					DoExprNode(exprSafeCall.MeassageArgument);
					_sb.Append(", ");
				} else {
					_sb.Append("0, 0, ");
				}

				_sb.Append(exprSafeCall.Parameters.Count);

				foreach (ExprNode node in exprSafeCall.Parameters) {
					WriteTypeParam(node.LanguageType);
				}

				foreach (ExprNode node in exprSafeCall.ParamNames) {
					WriteSafeParam(node);
				}

				foreach (ExprNode node in exprSafeCall.Parameters) {
					WriteSafeParam(node);
				}

			}

			_sb.Append(')');
		
		}



		private void WriteSafeParam(ExprNode exprNode) {
			_sb.Append(", ");
			if ((exprNode.LanguageType.Modifier & Modifier.Ret) != 0) {
				_sb.Append('&');
				DoExprNode(exprNode, true);
			} else {
				DoExprNode(exprNode);
			}
		}


		private void WriteTypeParam(LanguageType type) {
			_sb.Append(", CLT(");
			_sb.Append((int)type.BasicType);
			if (type.DimensionsCount != 0 || type.Modifier != Modifier.None || type.BasicType == BasicType.Object) {
				_sb.Append(',');
				_sb.Append((int)type.Modifier);
				_sb.Append(',');

				if (type.BasicType == BasicType.Object) {
					if (type.ObjectType == null) {
						_sb.Append("KerMain->Object,");
					} else {
						WriteKsidName(type.ObjectType);
						_sb.Append(',');
					}
				} else {
					_sb.Append("0,");
				}

				_sb.Append(type.DimensionsCount);
			}
			_sb.Append(")");
		}


		private void WriteSafeCallParams(ExprSafeCall exprSafeCall) {
			_sb.Append(exprSafeCall.Token.PositionInLines.FirstRow);
			_sb.Append(", ");

			if (exprSafeCall.CallType == ExprCallType.New) {
				_sb.Append("0, ");
				DoExprNode(exprSafeCall.Obj);
			} else {
				if (exprSafeCall.CallType == ExprCallType.Static) {
					_sb.Append("KerMain->StaticData, ");
				} else if (exprSafeCall.Obj == null) {
					_sb.Append("thisO, ");
				} else {
					DoExprNode(exprSafeCall.Obj);
					_sb.Append(", ");
				}
				DoExprNode(exprSafeCall.Method);
			}

		}




		private void DoObjectAccess(ExprObjectAccess exprObjectAccess) {
			if (exprObjectAccess.Child == null) {
				_sb.Append("thisO");
			} else {
				DoExprNode(exprObjectAccess.Child);
			}
			_sb.Append(".get<");
			_sb.Append(Utils.ParseType(exprObjectAccess.LanguageType));
			_sb.Append(">(Script->");
			_sb.Append(_ksWriter.KSNames.GetMemberVariable(exprObjectAccess.ObjectType, exprObjectAccess.VariableName));
			_sb.Append(')');
		}



		private void DoLocalVariable(ExprLocalVariable exprLocalVariable) {
			if (exprLocalVariable.Variable.Parameter != null && !CompilerConstants.IsNameTypeDirectMethod(_methodField.Name.NameType)) {
				_sb.Append("ctx.prm<");
				_sb.Append(Utils.ParseType(exprLocalVariable.LanguageType));
				_sb.Append(">(");
				_sb.Append(exprLocalVariable.Variable.Index);
				_sb.Append(')');
			} else {
				WriteVariableName(exprLocalVariable.Variable.Name);
			}
		}



		private void DoKsidName(ExprKsidName exprKsidName) {
			WriteKsidName(exprKsidName.Name);
		}



		private void DoDirectCall(ExprDirectCall exprDirectCall) {
			_sb.Append("Script->");
			_sb.Append(_ksWriter.KSNames.GetDirectMethod(exprDirectCall.MethodName));
			
			_sb.Append("(KerMain, ");

			if (CompilerConstants.IsNameTypeStatic(exprDirectCall.MethodName.NameType)) {
				_sb.Append("0");
			} else {
				if (exprDirectCall.Obj != null) {
					DoCast(new LanguageType(BasicType.Object, Modifier.None, 0, exprDirectCall.MethodName.DMInheritedFrom), OperatorArgumentCasts.CastObjectToBase, exprDirectCall.Obj);
				} else {
					if (_thisClassName == exprDirectCall.MethodName.DMInheritedFrom) {
						_sb.Append("thisO");
					} else {
						_sb.Append("thisO.Cast(Script->");
						_sb.Append(_ksWriter.KSNames.GetCast(_thisClassName, exprDirectCall.MethodName.DMInheritedFrom));
						_sb.Append(')');
					}

				}
			}

			for (int f = 0; f < exprDirectCall.Parameters.Count; f++) {
				_sb.Append(", ");
				WriteDirectParam(exprDirectCall.MethodName.ParameterLists[0][f].Type, exprDirectCall.ParametersCasts[f], exprDirectCall.Parameters[f]);
			}

			_sb.Append(')');
		}



		private void DoConstant(ExprConstant exprConstant) {
			switch (exprConstant.Token.Type) {
				case LexicalTokenType.Double:
					_sb.Append(exprConstant.Token.Double.ToString(CultureInfo.InvariantCulture));
					return;
				case LexicalTokenType.Char:
					_sb.Append(((int)exprConstant.Token.Char).ToString(CultureInfo.InvariantCulture));
					return;
				case LexicalTokenType.Int:
					_sb.Append(exprConstant.Token.Int.ToString(CultureInfo.InvariantCulture));
					return;
				case LexicalTokenType.String:
					WriteStringConstant(exprConstant.Token.Text);
					return;
				case LexicalTokenType.Keyword:
					switch (exprConstant.Token.Keyword.ConstantKeyword) {
						case ConstantKeyword.False:
							_sb.Append("false");
							return;
						case ConstantKeyword.Null:
							_sb.Append("Null");
							return;
						case ConstantKeyword.Sender:
							_sb.Append("ctx.Sender");
							return;
						case ConstantKeyword.This:
							_sb.Append("thisO");
							return;
						case ConstantKeyword.True:
							_sb.Append("true");
							return;
						// todo everything + nothing
						default:
							throw new InternalCompilerException();
					}
				default:
					throw new InternalCompilerException();
			}
		}



		private void DoBinary(ExprBinary exprBinary) {
			DoExprNode(exprBinary.Left);
			WriteSpace();
			_sb.Append(exprBinary.Token.Operator.Text);
			WriteSpace();
			DoCast(exprBinary.Left.LanguageType, exprBinary.ArgumentCasts, exprBinary.Right);
		}



		private void DoArrayAccess(ExprArrayAccess exprArrayAccess, bool lockArray) {
			DoExprNode(exprArrayAccess.Array);
			if (lockArray) {
				_sb.Append("->lAt(");
				DoCast(new LanguageType(BasicType.Int), exprArrayAccess.Cast, exprArrayAccess.Index);
				_sb.Append(')');
			} else {
				_sb.Append('[');
				DoCast(new LanguageType(BasicType.Int), exprArrayAccess.Cast, exprArrayAccess.Index);
				_sb.Append(']');
			}
		}



		private void DoArrayConstruction(ExprArray exprArray) {
			_sb.Append('(');
			_sb.Append(Utils.ParseType(exprArray.LanguageType));
			_sb.Append("().Create(");
			_sb.Append(exprArray.Fields.Count);
			_sb.Append(')');

			LanguageType type = exprArray.LanguageType;
			type.DimensionsCount--;
			int f=0;
			
			foreach (ExprNode node in exprArray.Fields) {
				_sb.Append(" << (");
				DoCast(type, exprArray.Casts[f], node);
				_sb.Append(')');
				f++;
			}

			_sb.Append(')');
		}





		private void DoDeclaration(LocalDeclaration localDeclaration) {
			_sb.Append(Utils.ParseType(localDeclaration.LanguageType));
			WriteSpace();
			WriteVariableName(localDeclaration.Name);

			if (localDeclaration.Initialization != null) {
				_sb.Append(" = ");
				DoExpression(localDeclaration.Initialization, localDeclaration.LanguageType);
			}

			_sb.Append(';');
		}







		private void WriteSpace()
		{
 			_sb.Append(' ');
		}



		private void WriteVariableName(String name) {
			_sb.Append("_KSL_");
			_sb.Append(name);
		}


		private void WriteTempVariable(int num) {
			_sb.Append("_KST_");
			_sb.Append(num);
		}


		private void WriteKsidName(KsidName name) {
			_sb.Append("Script->");
			_sb.Append(_ksWriter.KSNames.GetKsidName(name));
		}



		private void WriteStringConstant(String text) {
			_sb.Append("KString(L\"");
			// todo: conversion is not fully correct
			foreach (char ch in text) {
				switch (ch) {
					case '"':
						_sb.Append("\\\"");
						break;
					case '\\':
						_sb.Append("\\\\");
						break;
					case '\0':
						_sb.Append("\\0");
						break;
					case '\a':
						_sb.Append("\\a");
						break;
					case '\b':
						_sb.Append("\\b");
						break;
					case '\f':
						_sb.Append("\\f");
						break;
					case '\n':
						_sb.Append("\\n");
						break;
					case '\r':
						_sb.Append("\\r");
						break;
					case '\t':
						_sb.Append("\\t");
						break;
					case '\v':
						_sb.Append("\\v");
						break;
					default:
						_sb.Append(ch);
						break;
				}
			}
			_sb.Append("\")");
		}


	}


}
