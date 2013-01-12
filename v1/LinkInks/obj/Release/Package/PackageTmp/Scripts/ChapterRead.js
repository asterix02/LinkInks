window.$myCache =
{
    bookOutline:                $("#bookOutline"),
    discussionsControl:         $("#discussionsControl"),
    discussionsSlideout:        $("#discussionsSlideout"),
    discussionsSubmitAction:    $("#discussionsSubmitAction"),
    newDiscussionControl:       $("#newDiscussionControl"),
    newDiscussionTextControl:   $("#newDiscussionText"),
    selectedDiscussionId:       $("#selectedDiscussionId"),
    selectedModuleId:           $("#selectedModuleId"),
    submitAction:               $("#submitAction"),

    newPostTextHiddenInput:     $("#newPostText"),
    submitButtonOnEnterId:      "",

    submitValueAddDiscussion:   "addNewDiscussion",
    submitValueAddPost:         "addNewPost",
    submitValueAnswerQuiz:      "answerQuiz",
    submitValuePreviousPage:    "viewPreviousPage",
    submitValueNextPage:        "viewNextPage",
    submitValueRefresh:         "refreshDiscussions",

    discussionsCloseId:         "discussionsCloseButton",
    discussionsStartNewId:      "discussionsStartNewButton",
    fullScreenId:               "fullScreenButton",
    newDiscussionsSubmitId:     "newDiscussionsSubmitButton",
    newDiscussionsCancelId:     "newDiscussionsCancelButton",
    nextPageId:                 "nextPageButton",
    previousPageId:             "previousPageButton",

    prefixAddPost:              "addPost_",
    prefixAnswerSubmit:         "submitAnswer_",
    prefixAnswerCancel:         "cancelAnswer_",
    prefixDiscussionTab:        "discussionTab_",
    prefixDiscussionTape:       "discussionTape_",
    prefixModuleParagraph:      "moduleParagraph_",
    prefixNewPost:              "newPost_",
    prefixNewPostSubmit:        "newPostSubmitButton_",
    prefixNewPostText:          "newPostText_",
    prefixNewPostCancel:        "newPostCancelButton_",
    prefixViewAllPosts:         "viewAllPosts_"
};

var EnterKeyCode = 13;

$(function () {
    $myCache.bookOutline.on("click", function (event) {

        // Identify the child element that raised this event
        var target = $(event.target);
        $target = $(target);
        var $targetId = $target.attr("id");

        if ($targetId == null) {
            return;
        }

        // On "Full Screen" click, toggle full-screen mode
        else if ($targetId == $myCache.fullScreenId) {
            if (document.fullscreen || document.mozFullScreen || document.webkitIsFullScreen) {
                if (document.exitFullscreen) {
                    document.exitFullscreen();
                }
                else if (document.mozCancelFullScreen) {
                    document.mozCancelFullScreen();
                }
                else if (document.webkitCancelFullScreen) {
                    document.webkitCancelFullScreen();
                }
            }
            else {
                var docElm = document.documentElement;
                if (docElm.requestFullscreen) {
                    docElm.requestFullscreen(Element.ALLOW_KEYBOARD_INPUT);
                }
                else if (docElm.mozRequestFullScreen) {
                    docElm.mozRequestFullScreen(Element.ALLOW_KEYBOARD_INPUT);
                }
                else if (docElm.webkitRequestFullScreen) {
                    docElm.webkitRequestFullScreen(Element.ALLOW_KEYBOARD_INPUT);
                }
            }
        }

        // On "Previous Page" click, set the "submitAction" value
        else if ($targetId == $myCache.previousPageId) {
            $myCache.submitAction.val($myCache.submitValuePreviousPage);
        }

        // On "Next Page" click, set the "submitAction" value
        else if ($targetId == $myCache.nextPageId) {
            $myCache.submitAction.val($myCache.submitValueNextPage);
        }

        // On "DiscussionTab" click, slide out the DiscussionsSlideout
        else if ($targetId.substr(0, $myCache.prefixDiscussionTab.length) == $myCache.prefixDiscussionTab) {
            var moduleId = $targetId.replace($myCache.prefixDiscussionTab, "");
            slideOutDiscussions(moduleId);
        }

        // On "DiscussionTape" click, slide out the DiscussionsSlideout
        else if ($targetId.substr(0, $myCache.prefixDiscussionTape.length) == $myCache.prefixDiscussionTape) {
            var moduleId = $targetId.replace($myCache.prefixDiscussionTape, "");
            slideOutDiscussions(moduleId);
        }
    });
});

function slideOutDiscussions(moduleId) {
    // Submit the DiscussionsControl form to trigger an asynchronous fetch of that module's discussions
    $myCache.selectedModuleId.val(moduleId);
    $myCache.discussionsSubmitAction.val($myCache.submitValueRefresh);
    $myCache.discussionsControl.submit();

    // Bring the discussions into view
    $myCache.bookOutline.animate({
        marginLeft: -350
    }, 400);

    $myCache.discussionsSlideout.show("slide");
}

// Attaching handlers for each discussion or post is extremely expensive at runtime. Instead, we attach one
// handler at the parent level (discussionsSlideOut), and trap all the bubbled-up events.
$(function () {
    $myCache.discussionsSlideout.on("click", function (event) {

        // Identify the child element that raised this event
        var target = $(event.target);
        $target = $(target);
        var $targetId = $target.attr("id");

        if ($targetId == null) {
            return;
        }

        // On "View All Posts" click, slide open/close the "posts_id" element
        else if ($targetId.substr(0, $myCache.prefixViewAllPosts.length) == $myCache.prefixViewAllPosts) {
            var actionElement = "#" + $targetId.replace($myCache.prefixViewAllPosts, "posts_");
            $(actionElement).slideToggle("fast");
        }

        // On "Hide All Discussions" click, hide the "discussionSlideOut" element (which happens to be $this)
        else if ($targetId == $myCache.discussionsCloseId) {
            $myCache.selectedModuleId.val(-1);
            $(this).hide("slide");

            $myCache.bookOutline.animate({ marginLeft: 0 }, 200);
        }

        // On "Add Post" click, slide open/close the "newPost_id" element, and set the right focus interactions
        else if ($targetId.substr(0, $myCache.prefixAddPost.length) == $myCache.prefixAddPost) {
            var actionElement = "#" + $targetId.replace($myCache.prefixAddPost, $myCache.prefixNewPost);
            $(actionElement).slideToggle("fast");

            $myCache.submitButtonOnEnterId = $targetId.replace($myCache.prefixAddPost, $myCache.prefixNewPostSubmit);

            var newPostTextElement = "#" + $targetId.replace($myCache.prefixAddPost, $myCache.prefixNewPostText);
            $(newPostTextElement).focus();
        }

        // On "New Post Submit" click, set the "selectedDiscussionId", "newPostText"
        else if ($targetId.substr(0, $myCache.prefixNewPostSubmit.length) == $myCache.prefixNewPostSubmit) {
            var discussionId = $targetId.replace($myCache.prefixNewPostSubmit, "");
            $myCache.selectedDiscussionId.val(discussionId);

            var newPostTextValue = $("#" + $myCache.prefixNewPostText + discussionId).val();
            $myCache.newPostTextHiddenInput.val(newPostTextValue);

            // Set the submit's action value to "Add Post" so that the right form action will trigger on click/Enter
            $myCache.discussionsSubmitAction.val($myCache.submitValueAddPost);
        }

        // On "New Post Cancel" click, slide close the "newPost_id" element
        else if ($targetId.substr(0, $myCache.prefixNewPostCancel.length) == $myCache.prefixNewPostCancel) {
            var actionElement = "#" + $targetId.replace($myCache.prefixNewPostCancel, "newPost_");
            $(actionElement).slideToggle("fast");
        }

        // On "Start New Discussion" click, slide open/close the "newDiscussionControl" element
        else if ($targetId == $myCache.discussionsStartNewId) {
            $myCache.newDiscussionControl.slideToggle("fast");

            $myCache.submitButtonOnEnterId = $myCache.newDiscussionsSubmitId;
            $myCache.discussionsSubmitAction.val($myCache.submitValueAddDiscussion);

            $myCache.newDiscussionTextControl.focus();
        }

        // On "Submit New Discussion" click, set the "submitAction" value to "Add Discussion" 
        else if ($targetId == $myCache.newDiscussionsSubmitId) {
            $myCache.discussionsSubmitAction.val($myCache.submitValueAddDiscussion);
        }

        // On "Cancel New Discussion" click, slide close the "newDiscussionControl" element
        else if ($targetId == $myCache.newDiscussionsCancelId) {
            $myCache.newDiscussionControl.slideToggle("fast");
        }

        // On "Submit Answer" click, set the "discussionsSubmitAction" values
        else if ($targetId.substr(0, $myCache.prefixAnswerSubmit.length) == $myCache.prefixAnswerSubmit) {
            $myCache.discussionsSubmitAction.val($myCache.submitValueAnswerQuiz);
        }

        // On "Cancel Answer" click, hide the "discussionSlideOut" element (which happens to be $this)
        else if ($targetId.substr(0, $myCache.prefixAnswerCancel.length) == $myCache.prefixAnswerCancel) {
            $myCache.selectedModuleId.val(-1);
            $(this).hide("slide");

            var tabElement = "#" + $myCache.prefixDiscussionTab + $myCache.selectedModuleId.val();
            $(tabElement).fadeTo(200, 0.6); ;

            $myCache.bookOutline.animate({ marginLeft: 0 }, 200);
        }
    });
});

// Attaching handlers for each discussion or post is extremely expensive at runtime. Instead, we attach one
// handler at the parent level (discussionsSlideOut), and trap all the bubbled-up events.
$(function () {
    $myCache.discussionsSlideout.on("keypress", function (event) {

        // Identify the child element that raised this event
        var target = $(event.target);
        $target = $(target);
        var $targetId = $target.attr("id");

        if ($targetId == null) {
            return;
        }

        // Submit the DiscussionsControl form, and trigger an asynchronous refresh
        else if (event.which == EnterKeyCode) {
            event.preventDefault(); // if we don't do this, it will send another click event to the target

            // We could simply call $myCache.discussionsControl.submit() if all the information about the new inputs
            // was already set. In the case of new discussion, it is set, but it isn't in the case of new post.
            if ($myCache.submitButtonOnEnterId.substr(0, $myCache.prefixNewPostSubmit.length) == $myCache.prefixNewPostSubmit) {
                var discussionId = $myCache.submitButtonOnEnterId.replace($myCache.prefixNewPostSubmit, "");
                $myCache.selectedDiscussionId.val(discussionId);

                var newPostTextId = ("#" + $myCache.prefixNewPostText + discussionId);
                $myCache.newPostTextHiddenInput.val($(newPostTextId).val());

                // Set the submit's action value to "Add Post" so that the right form action will trigger on Enter
                $myCache.discussionsSubmitAction.val($myCache.submitValueAddPost);
            }

            $myCache.discussionsControl.submit();
        }
    });
});

