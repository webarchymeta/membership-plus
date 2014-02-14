function tokenNameMap(tk, entity, isquery) {
    switch (entity) {
        case 'User':
            {
                if (!isquery) {
                    return tk.TkName.indexOf('Password') == -1;
                } else {
                    return tk.TkName.indexOf('Password') == -1 || tk.TkName.indexOf('Password') != -1 && tk.TkName.indexOf('Failed') != -1;
                }
            }
            break;
    }
    return true;
}