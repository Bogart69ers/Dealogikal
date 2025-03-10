using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dealogikal.Database;
using Dealogikal.Utils;

namespace Dealogikal.Repository
{
    public class TodoListManager
    {
        private BaseRepository<todoLists> _todo;


        public TodoListManager()
        {
            _todo = new BaseRepository<todoLists>();
        }

        public List<todoLists> GetAllTodo()
        {
            return _todo.GetAll();
        }

        public todoLists GetTodobyEmployeeId(string employeeId)
        {
            return _todo._table.FirstOrDefault(e => e.employeeId == employeeId);
        }

        public ErrorCode Createtodo(todoLists td, ref string errMsg)
        {
            try
            {
                if (_todo.Create(td, out errMsg) != ErrorCode.Error)
                {
                    return ErrorCode.Error;
                }

                return ErrorCode.Success;
            }
            catch (Exception ex)
            {

                errMsg = ex.Message;
                return ErrorCode.Error;
            }
        }
    }
}