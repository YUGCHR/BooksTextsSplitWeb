import React from "react";
import { connect } from "react-redux";
import { compose } from "redux";
import {
  toggleIsSelectingBookId,
  toggleIsSelectingUploadVersion,
  toggleIsQuickViewBooksPair,
  fetchAllBookIdsWithNames,
  fetchAllVersionsOfSelectedBook,
  fetchChosenVersionOfSelectedBooksPair,
} from "../../redux/select-reducer";
import SelectTexts from "./SelectTexts";
import Preloader from "../common/preloader/Preloader";

class SelectTextsContainerAPI extends React.Component {
  constructor(props) {
    super(props);
  }

  componentDidMount() {
    this.props.fetchAllBookIdsWithNames();
  }

  /* failureCallback = () => {
    console.log(this.props.maxUploadedVersion);
  }; */

  /* fetchAllBookIdsWithNames = () => {
    let where = "bookSentenceId";
    let whereValue = 1;
    let startUpVersion = 1;
    return this.props.getAllBookIdsWithNamesThunk(where, whereValue, startUpVersion)
      .then((rrr) => {
        console.log("after: getAllBookIdsWithNamesThunk", rrr);
      })
      .catch(this.failureCallback);
  }; */

  /* fetchAllVersionsOfSelectedBook = (bookId) => {
    let where = "bookSentenceId";
    let whereValue = 1;
    return this.props.getAllBookNameVersionsThunk(where, whereValue, bookId)
      .then((Response) => {
        console.log("getAllBookNameVersionsThunk: finished", Response);
      })
      .catch(this.failureCallback);
  }; */

 /*  fetchChosenVersionOfSelectedBooksPair = (selectedBookId, selectedVersion) => {
    let where1 = "bookId";
    let where1Value = selectedBookId;
    let where2 = "uploadVersion";
    let where2Value = selectedVersion;
    return this.props.getBooksPairTextsThunk(where1, where1Value, where2, where2Value)
      .then((data) => {
        console.log("Response of getBooksPairTexts", data);
      })
      .catch(this.failureCallback);
  }; */

  //Let to switch on BooksNames choosing (return to the previous) - subPage 01
  switchBooksIdsOn = () => {
    this.props.toggleIsSelectingUploadVersion(false); //subPage 02
    this.props.toggleIsQuickViewBooksPair(false); //subPage 03
    this.props.fetchAllBookIdsWithNames().then((r) => {
      this.props.toggleIsSelectingBookId(true); //subPage 01
    });
    return 0;
  };

  //Let to switch on BookVersions choosing - subPage 02
  switchBookVersionsOn = (bookId) => {
    this.props.toggleIsSelectingBookId(false); //subPage 01
    this.props.toggleIsQuickViewBooksPair(false); //subPage 03
    this.props.fetchAllVersionsOfSelectedBook(bookId).then((r) => {
      this.props.toggleIsSelectingUploadVersion(true); //subPage 02
    });
    return { bookId };
  };

  //Let to switch on QuickView - //subPage 03
  switchQuickViewOn = (selectedBookId, selectedVersion) => {
    this.props.toggleIsSelectingBookId(false); //subPage 01
    this.props.toggleIsSelectingUploadVersion(false); //subPage 02
    this.props.fetchChosenVersionOfSelectedBooksPair(selectedBookId, selectedVersion).then((r) => {
      this.props.toggleIsQuickViewBooksPair(true); //subPage 03
      return { selectedVersion };
    });
  };

  //Let to switch on NEXT choosing - //subPage 04
  nextAfterQuickView = (i) => {
    this.props.toggleIsSelectingUploadVersion(false); //subPage 01
    this.props.toggleIsQuickViewBooksPair(false); //subPage 02
    this.props.toggleIsQuickViewBooksPair(false); //subPage 03

    return { i };
  };

  render() {
    return (
      <>
        {this.props.isFetching ? <Preloader /> : null}
        <SelectTexts
          sentencesCount={this.props.sentencesCount}
          allBookNamesSortedByIds={this.props.allBookNamesSortedByIds}
          allVersionsOfBooksNames={this.props.allVersionsOfBooksNames}
          booksPairTexts={this.props.booksPairTexts}
          bookNamesVersion1SortedByIds={this.props.bookNamesVersion1SortedByIds}
          isSelectingBookId={this.props.isSelectingBookId}
          isSelectingUploadVersion={this.props.isSelectingUploadVersion}
          isFetching={this.props.isFetching}
          switchBooksIdsOn={this.switchBooksIdsOn}
          switchBookVersionsOn={this.switchBookVersionsOn}
          switchQuickViewOn={this.switchQuickViewOn}
          nextAfterQuickView={this.nextAfterQuickView}
          isQuickViewBooksPair={this.props.isQuickViewBooksPair}
        />
      </>
    );
  }
}

let mapStateToProps = (state) => {
  return {
    sentencesCount: state.selectTextsPage.sentencesCount,
    isSelectingBookId: state.selectTextsPage.isSelectingBookId,
    isSelectingUploadVersion: state.selectTextsPage.isSelectingUploadVersion,
    isFetching: state.selectTextsPage.isFetching,
    allBookNamesSortedByIds: state.selectTextsPage.allBookNamesSortedByIds,
    bookNamesVersion1SortedByIds: state.selectTextsPage.bookNamesVersion1SortedByIds,
    allVersionsOfBooksNames: state.selectTextsPage.allVersionsOfBooksNames,
    booksPairTexts: state.selectTextsPage.booksPairTexts,
    isQuickViewBooksPair: state.selectTextsPage.isQuickViewBooksPair,
  };
};

export default compose(
  connect(mapStateToProps, {
    toggleIsSelectingBookId,
    toggleIsSelectingUploadVersion,
    toggleIsQuickViewBooksPair,    
    fetchAllBookIdsWithNames,
    fetchAllVersionsOfSelectedBook,
    fetchChosenVersionOfSelectedBooksPair,
  })
)(SelectTextsContainerAPI);
