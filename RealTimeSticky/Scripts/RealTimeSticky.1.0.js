var realTimeStickyApp = angular.module('realTimeStickyApp', [])
    .value('$', $)
    .service('noteService', ['$rootScope', '$', function ($rootScope, $)
    {
        var connection = null;
        var proxy = null;
        var data = [];
        var connect = function ()
        {
            //Getting the connection object
            connection = $.hubConnection();

            //Creating proxy
            proxy = connection.createHubProxy('noteHub');

            //Starting connection
            connection.start().done(function ()
            {
                console.log('Now connected, connection ID=' + connection.id);
                getNotesFromServer();
            })
            .fail(function ()
            {
                console.log('Could not connect');
            });

            proxy.on('AddNotesServer', function (notes)
            {
                if (notes &&
                    notes.length > 0)
                {
                    $rootScope.$apply(function ()
                    {
                        for (var i = 0; i < notes.length; i++)
                        {
                            var index = getNoteIndex(notes[i].ID);
                            if (index === -1)
                            {
                                data.push(notes[i]);
                            }
                        }
                    });
                }
            });

            proxy.on('AddNoteServer', function (note)
            {
                if (note)
                {
                    var index = getNoteIndex(note.ID);
                    if (index === -1)
                    {
                        toastr.info("Incoming change");
                        $rootScope.$apply(function ()
                        {
                            data.push(note);
                        });
                    }
                }
            });

            proxy.on('UpdateNoteServer', function (note)
            {
                if (note)
                {
                    var index = getNoteIndex(note.ID);
                    if (index > -1)
                    {
                        toastr.info("Incoming update");
                        $rootScope.$apply(function ()
                        {
                            found = true;
                            data[index] = note;
                        });
                    }
                    else
                    {
                        $rootScope.$apply(function ()
                        {
                            data.push(note);
                        });
                    }
                }
            });

            proxy.on('RemoveNoteServer', function (id)
            {
                if (data &&
                    data.length > 0)
                {
                    var foundIndex = getNoteIndex(id);
                    if (foundIndex != -1)
                    {
                        $rootScope.$apply(function ()
                        {
                            data.splice(foundIndex, 1);
                        });
                    }
                }
            });
        };

        var getNoteIndex = function (noteId)
        {
            var index = -1;
            if (noteId > -1 &&
                data &&
                data.length > 0)
            {
                for (var i = 0; i < data.length; i++)
                {
                    if (data[i].ID === noteId)
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        };

        var getNotesFromServer = function ()
        {
            proxy.invoke('getNotes');
        };

        var getNotes = function ()
        {
            return data;
        };

        var addNewNote = function ()
        {
            proxy.invoke('addNewNote');
        };

        var updateNote = function (note)
        {
            proxy.invoke('updateNote', note);
        };

        var removeNote = function (id)
        {
            proxy.invoke('removeNote', id);
        };

        return {
            connect: connect,
            addNewNote: addNewNote,
            updateNote: updateNote,
            removeNote: removeNote,
            getNotes: getNotes
        };
    }])
    .controller('stickyController', ['$scope', '$window', 'noteService', function ($scope, $window, noteService)
    {
        $scope.SelectedIndex = -1;
        noteService.connect();

        $scope.getNotes = function ()
        {
            return noteService.getNotes();
        };

        $scope.addNewNote = function ()
        {
            noteService.addNewNote();
        };

        $scope.removeNote = function (id)
        {
            if ($window.confirm("Are you sure ?"))
            {
                noteService.removeNote(id);
                //toastr.success("Deleted");
            }
        };

        $scope.gotFocus = function (index)
        {
            $scope.SelectedIndex = index;
        };

        $scope.lostFocus = function (note)
        {
            $scope.SelectedIndex = -1;
            noteService.updateNote(note);
            toastr.success("Saved");
        };

        $scope.keyup = function (e)
        {
            $scope.resizeEditableArea($(e.target));
        };

        $scope.$on('ngRepeatFinished', function (ngRepeatFinished)
        {
            $(".editableArea").each(function (index, value)
            {
                $scope.resizeEditableArea($(value));
            });
        });

        $scope.resizeEditableArea = function ($box)
        {
            //toastr.info('resizing');
            if ($box.prop("clientHeight") < $box.prop("scrollHeight"))
            {
                //has scroll
                var newHeight = $box.prop("scrollHeight") + "px";
                $box.css("height", newHeight);
            }
        }
    }])
    .directive('draggable', function ()
    {
        return {
            // A = attribute, E = Element, C = Class and M = HTML Comment
            restrict: 'A',
            link: function (scope, element, attrs)
            {
                element.draggable({
                    //snap: true,
                    //snapMode: "outer",
                    cursor: "move",
                    handle: ".panel-heading",
                    containment: ".mainContainer .row",
                    cancel: ".btnRemove, .editableArea",
                    stop: function (event, ui)
                    {
                        var $innerScope = ui.helper.scope();
                        $innerScope.$apply(function ()
                        {
                            //toastr.info("position updated");
                            $innerScope.item.Left = ui.position.left;
                            $innerScope.item.Top = ui.position.top;
                            $innerScope.$parent.lostFocus($innerScope.item);
                        });
                    }
                });
                //.resizable(); //jquery-ui.css needed for this to work
            }
        };
    })
    .directive('onFinishRenderTextarea', function ($timeout)
    {
        return {
            restrict: 'A',
            link: function (scope, element, attr)
            {
                $timeout(function ()
                {
                    scope.$emit('ngRepeatFinished');
                });
            }
        }
    }
);