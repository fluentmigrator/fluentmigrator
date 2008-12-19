using System;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator
{
	public interface IMigrationGenerator
	{
		void SetTypeMap(DbType type, string template);
		void SetTypeMap(DbType type, string template, int maxSize);
		string GetTypeMap(DbType type, int size, int precision);

		string Generate(CreateTableExpression expression);
		string Generate(CreateColumnExpression expression);
		string Generate(DeleteTableExpression expression);
		string Generate(DeleteColumnExpression expression);
		string Generate(CreateForeignKeyExpression expression);
		string Generate(DeleteForeignKeyExpression expression);
		string Generate(CreateIndexExpression expression);
		string Generate(DeleteIndexExpression expression);
		string Generate(RenameTableExpression expression);
		string Generate(RenameColumnExpression expression);
	}
}
