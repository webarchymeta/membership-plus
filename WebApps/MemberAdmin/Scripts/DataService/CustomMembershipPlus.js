function tokenNameMap(tk, entity, isquery) {
    mapCommonTokens(tk, isquery);
    switch (entity) {
        case 'User':
            {
                mapUserTokens(tk, isquery);
                if (!isquery && (tk.TkName.toLowerCase().indexOf('password'.toLowerCase()) != -1))
                    return false;
                else if (isquery && (tk.TkName.toLowerCase().indexOf('password'.toLowerCase()) != -1))
                    return false;
                else
                    return true;
            }
            break;
    }
    return true;
}

function mapCommonTokens(tk, isquery) {
    switch (tk.TkName) {
        case "&&":
            tk.DisplayAs = 'and';
            break;
        case "||":
            tk.DisplayAs = 'or';
            break;
        case "asc":
            tk.DisplayAs = 'asc';
            break;
        case "desc":
            tk.DisplayAs = 'desc';
            break;
        default:
            break;
    }
}

function mapUserTokens(tk, isquery) {
    switch (tk.TkName) {
        case "UserAppMember.":
            tk.DisplayAs = 'Membership.';
            break;
        case "UserDetail.":
            tk.DisplayAs = 'Detail.';
            break;
        case "TextContent":
            tk.DisplayAs = 'keywords';
            break;
        case "AddressInfo":
            tk.DisplayAs = 'Address';
            break;
        case "Username":
            tk.DisplayAs = 'Name';
            break;
        default:
            break;
    }
}