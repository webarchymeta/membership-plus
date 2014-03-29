var appRoot = null;
var appName = null;
var roleSet = null;
var alreadyInRoleMsg = 'The user is already in the role!';

function RoleAbs(data) {
    var self = this;
    self.Initializing = true;
    self.data = data;
    self.ID = ko.observable(data == null ? 0 : data.id);
    self.RoleName = ko.observable(data == null ? "" : data.name);
    self.DistinctString = ko.observable(data == null ? "" : data.path);
    self.RolePriority = ko.observable(data == null ? 0 : data.priority);
    self.ParentID = ko.observable(data == null ? null : data.pid);
    self.ParentExists = ko.observable(data == null ? true : data.hasParents);
    self.ChildExists = ko.observable(data == null ? false : data.hasChilds);
    self.IsChildsLoaded = ko.observable(data == null ? false : data.childsLoaded);
    self.IsNodeSelected = ko.observable(false);
    self.IsNodeExpanded = ko.observable(false);
    self.IsPlaceHolder = ko.observable(false);
    self.ParentEntity = ko.observable(null);
    self.ChildEntities = ko.observableArray([]);
    if (data != null && data.childsLoaded) {
        for (var i = 0; i < data.childs.length; i++) {
            var c = new RoleAbs(data.childs[i]);
            self.ChildEntities.push(c);
            c.ParentEntity(self);
        }
    }
    self.UsersInRole = ko.observableArray([]);
    self.NewChild = ko.observable();
    self.InitChildPlaceholder = function () {
        self.NewChild(new RoleAbs(null));
        self.NewChild().ParentEntity(self);
        self.NewChild().RolePriority(self.RolePriority() + (adminMaxPriority - self.RolePriority() < 10 ? adminMaxPriority - self.RolePriority() : 10));
    };

    if (data == null) {
        self.NewChild(null);
    } else {
        self.InitChildPlaceholder();
    }

    self.CanOp = data == null ? true : data.op;

    self.CanAdd = ko.computed(function () {
        if (!roleSet) {
            return true;
        }
        if (self.ParentEntity() == null) {
            var ilast = roleSet.HierarchyRoots().length - 1;
            return ilast >= 0 && self.RoleName() == roleSet.HierarchyRoots()[ilast].RoleName();
        } else {
            if (!self.ParentEntity().IsNodeExpanded()) {
                return false;
            }
            var ilast = self.ParentEntity().ChildEntities().length - 1;
            if (ilast == -1) {
                var lev = self.ParentEntity().RolePriority();
                return adminMaxPriority > self.ParentEntity().RolePriority();
            } else {
                return self.RoleName() == self.ParentEntity().ChildEntities()[ilast].RoleName() && adminMaxPriority > self.ParentEntity().RolePriority();
            }
        }
    });


    self.IsChangeValid = ko.computed(function () {
        if (self.ParentEntity() == null) {
            return adminMaxPriority - self.RolePriority() >= 0;
        } else {
            return self.RolePriority() > self.ParentEntity().RolePriority() && (adminMaxPriority - self.RolePriority()) >= 0;
        }
    });

    self.IsNewValid = ko.computed(function () {
        if (self.RoleName() == null || self.RoleName() == '') {
            return false;
        }
        if (self.ParentEntity() == null) {
            for (var i = 0; i < roleSet.HierarchyRoots().length; i++) {
                if (self.RoleName() == roleSet.HierarchyRoots()[i].RoleName()) {
                    return false;
                }
            }
            return adminMaxPriority - self.RolePriority() >= 0;
        } else {
            for (var i = 0; i < self.ParentEntity().ChildEntities().length; i++) {
                if (self.RoleName() == self.ParentEntity().ChildEntities()[i].RoleName()) {
                    return false;
                }
            }
            return self.RolePriority() > self.ParentEntity().RolePriority() && (adminMaxPriority - self.RolePriority()) >= 0;
        }
    });

    self.IsDirty = ko.computed(function () {
        return self.data != null && self.RolePriority() != self.data.priority;
    });

    self.CanDelete = ko.computed(function () {
        var ok = self.CanOp && !self.ChildExists();
        if (!ok || data == null) {
            return false;
        } else {
            for (var i = 0; i < adminRoleIds.length; i++) {
                if (data.id == adminRoleIds[i]) {
                    return false;
                }
            }
            return true;
        }
    });

    self.LoadRoleChildren = function (callback) {
        var a = self.IsChildsLoaded();
        var b = self.ChildExists();
        if (self.IsChildsLoaded() || !self.ChildExists()) {
            if (callback) {
                callback(true);
            }
            return;
        }
        $.ajax({
            url: appRoot + "RoleAdmin/LoadRoleChildren",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ pid: self.data.id }),
            beforeSend: function () {
            },
            success: function (content) {
                var data = content;
                self.ChildExists(data.length > 0);
                self.ChildEntities.removeAll();
                for (var i = 0; i < data.length; i++) {
                    var c = new RoleAbs(data[i]);
                    self.ChildEntities.push(c);
                    c.ParentEntity(self);
                    if (!c.ChildExists()) {
                        var dummy = new RoleAbs(null);
                        dummy.IsPlaceHolder(true);
                        dummy.ParentEntity(c);
                        c.ChildEntities.push(dummy);
                    }
                }
                self.IsChildsLoaded(true);
                if (data.length == 0) {
                    var dummy = new RoleAbs(null);
                    dummy.IsPlaceHolder(true);
                    dummy.ParentEntity(self);
                    self.ChildEntities.push(dummy);
                }
                if (callback) {
                    callback(true);
                }
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false);
            },
            complete: function () {
            }
        });
    };

    self.CreateNewRole = function (pid, callback) {
        $.ajax({
            url: appRoot + "RoleAdmin/CreateNewRole",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ name: self.RoleName(), priority: self.RolePriority(), pid: pid }),
            beforeSend: function () {
            },
            success: function (content) {
                var data = content;
                if (data.ok) {
                    if (callback) {
                        callback(true, data.role);
                    }
                } else {
                    alert(data.msg);
                    callback(false, null);
                }
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false, null);
            },
            complete: function () {
            }
        });
    };

    self.UpdateRole = function (pid, callback) {
        $.ajax({
            url: appRoot + "RoleAdmin/UpdateRole",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ id: self.ID(), priority: self.RolePriority(), pid: pid }),
            beforeSend: function () {
            },
            success: function (content) {
                var data = content;
                if (data.ok) {
                    if (callback) {
                        callback(true);
                    }
                } else {
                    alert(data.msg);
                    callback(false);
                }
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false);
            },
            complete: function () {
            }
        });
    };

    self.DeleteRole = function (callback) {
        $.ajax({
            url: appRoot + "RoleAdmin/DeleteRole",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ id: self.ID() }),
            beforeSend: function () {
            },
            success: function (content) {
                var data = content;
                if (data.ok) {
                    if (callback) {
                        callback(true);
                    }
                } else {
                    alert(data.msg);
                    callback(false);
                }
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false);
            },
            complete: function () {
            }
        });
    };

    self.ListUsersInRole = function (callback) {
        $.ajax({
            url: appRoot + "RoleAdmin/ListUsersInRole",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ id: self.ID() }),
            beforeSend: function () {
            },
            success: function (content) {
                var data = content;
                if (data.ok) {
                    self.UsersInRole.removeAll();
                    for (var i = 0; i < content.users.length; i++) {
                        self.UsersInRole.push(new Role(content.users[i]));
                    }
                    if (callback) {
                        callback(true, content.users);
                    }
                } else {
                    alert(data.msg);
                    callback(false, null);
                }
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false);
            },
            complete: function () {
            }
        });
    }

    self.Initializing = false;
}

function RoleSet() {
    var self = this;
    self.SetFilter = "";
    self.HierarchyRoots = ko.observableArray([]);
    self.HierarchyRootsLoaded = ko.observable(false);
    self.NewChild = ko.observable(new RoleAbs(null));
    self.CurrentSelectedRole = ko.observable(null);
    self.LoadRoleSetRoots = function () {
        if (self.HierarchyRootsLoaded()) {
            return;
        }
        $.ajax({
            url: appRoot + "RoleAdmin/LoadRoleSetRoots",
            type: "GET",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            /*data: JSON.stringify({ cntx: self.clientcontext }),*/
            beforeSend: function () {
            },
            success: function (content) {
                var data = content.LoadRoleSetRootsResult;
                self.HierarchyRoots.removeAll();
                for (var i = 0; i < data.length; i++) {
                    var r = new RoleAbs(data[i]);
                    r.NewChild.RolePriority(self.RolePriority() + 10);
                    self.HierarchyRoots.push(r);
                    if (!r.ChildExists()) {
                        var dummy = new RoleAbs(null);
                        dummy.IsPlaceHolder(true);
                        dummy.ParentEntity(r);
                        r.ChildEntities.push(dummy);
                    }
                }
                self.HierarchyRootsLoaded(true);
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
            },
            complete: function () {
            }
        });
    };


    self.LoadRoleFullHierarchyRecurs = function (entity, callback) {
        $.ajax({
            url: appRoot + "RoleAdmin/LoadRoleFullHierarchyRecurs",
            type: "POST",
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ cntx: self.clientcontext, entity: entity.data }),
            beforeSend: function () {
            },
            success: function (content) {
                var data = content.LoadRoleFullHierarchyRecursResult;
                var r = new RoleAbs(data);
                entity.HierarchyRoot(r);
                self.HierarchyRoots.removeAll();
                self.HierarchyRoots.push(r);
                self.HierarchyRootsLoaded(false);
                if (callback) {
                    callback(true);
                }
            },
            error: function (jqxhr, textStatus) {
                alert(jqxhr.responseText);
                callback(false);
            },
            complete: function () {
            }
        });
    };
}

function _loadChildNodes(data, event) {
    if (data.IsChildsLoaded()) {
        data.IsNodeExpanded(true);
    } else {
        $(event.target).addClass("waiting");
        data.LoadRoleChildren(function (ok) {
            if (ok) {
                data.IsNodeExpanded(true);
            }
            $(event.target).removeClass("waiting");
        });
    }
}

function _createNewRole(data, event) {
    $(event.target).addClass("waiting");
    var pid;
    if (typeof data.HierarchyRoots != 'undefined') {
        pid = null;
    } else {
        pid = data.ID();
    }
    data.NewChild().CreateNewRole(pid, function (ok, role) {
        if (ok) {
            if (typeof data.HierarchyRoots != 'undefined') {
                data.HierarchyRoots.push(new RoleAbs(role));
                data.NewChild(new RoleAbs(null));
            } else {
                var child = new RoleAbs(role);
                var dummy = new RoleAbs(null);
                dummy.IsPlaceHolder(true);
                dummy.ParentEntity(child);
                child.ChildEntities.push(dummy);
                if (data.ChildEntities().length == 1) {
                    var x = data.ChildEntities()[0];
                    if (x.IsPlaceHolder()) {
                        data.ChildEntities.remove(x);
                    }
                }
                data.ChildEntities.push(child);
                child.ParentEntity(data);
                if (!data.ChildExists()) {
                    data.ChildExists(true);
                }
                child.IsNodeExpanded(true);
                data.InitChildPlaceholder();
            }
        } else {
            alert(content.msg);
        }
        $(event.target).removeClass("waiting");
    });
}

function _updateRole(data, event) {
    if (data.RolePriority() == data.data.priority) {
        return;
    }
    $(event.target).addClass("waiting");
    var pid;
    if (data.ParentEntity() == null) {
        pid = null;
    } else {
        pid = data.ParentEntity().ID();
    }
    data.UpdateRole(pid, function (ok) {
        if (ok) {
            var val = data.RolePriority();
            data.RolePriority(data.data.priority);
            data.data.priority = val;
            data.RolePriority(val);
        }
        $(event.target).removeClass("waiting");
    });
}

function _deleteRole(data, event) {
    $(event.target).addClass("waiting");
    data.DeleteRole(function (ok) {
        if (ok) {
            if (data.ParentEntity() != null) {
                data.ParentEntity().ChildEntities.remove(data);
            } else {
                roleSet.HierarchyRoots.remove(data);
            }
        }
        $(event.target).removeClass("waiting");
    });
}


function _selectRole(data, event) {
    var old = roleSet.CurrentSelectedRole();
    if (old) {
        old.IsNodeSelected(false);
    }
    data.ListUsersInRole(function (ok, users) {
        if (ok) {
            userSet.IsRoleSelected(true);
            userSet.roleUsers = users;
        }
    });
    userSet.IsRoleSelected(true);
    roleSet.CurrentSelectedRole(data);
    userSet.CurrentSelectedRole(data);
    data.IsNodeSelected(true);
    userSet.IsQueryStateChanged(true);
    showlist(null);
}

function _addUserToCurrentRole(data, event) {
    if (!userSet.IsRoleSelected()) {
        return;
    }
    if (userSet.roleUsers) {
        for (var i = 0; i < userSet.roleUsers.length; i++) {
            if (userSet.roleUsers[i].id == data.data.ID) {
                alert(alreadyInRoleMsg);
                return;
            }
        }
    }
    var r = userSet.CurrentSelectedRole();
    data.AddToRole(r.data, null, function (ok, added) {
        if (ok) {
            var u = {
                id: data.data.ID,
                name: data.data.Username,
                path: r.data.path,
                level: added.level,
                op: true
            };
            roleSet.CurrentSelectedRole().UsersInRole.push(new Role(u));
            userSet.roleUsers.push(u);
        }
    });
    userSet.IsQueryStateChanged(true);
    showlist(null);
}