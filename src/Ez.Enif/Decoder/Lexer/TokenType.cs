using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Enif
{
    public enum TokenType
    {
        #region core
        LeftBracket,        // [
        RightBracket,       // ]
        Equal,              // =
        Semicolon,          // ;
        Error,              // error token
        Trash,              // trash token
        NewLine,            // new line
        EOF,                // End of file
        #endregion core

        LeftParent,         // (
        RightParent,        // )

        Plus,               // +
        Minus,              // -
        Slash,              // /
        Backslash,          // \
        Star,               // *

        String,
        Number,
        True,
        False,
        Identifier,

        Exclamation,        // !
        ExclamationEqual,   // !=
        EqualEqual,         // ==
        Greater,            // >
        GreaterEqual,       // >=
        Less,               // <
        LessEqual,          // >

        And,                // &
        Or,                 // |
    }
}
