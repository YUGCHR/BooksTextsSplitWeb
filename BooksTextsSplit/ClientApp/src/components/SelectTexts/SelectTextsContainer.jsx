import React from "react";
import Axios from "axios";
//import ReactScrollWheelHandler from "react-scroll-wheel-handler";
import { connect } from "react-redux";
import {
  setAllBookIdsWithNames,
  setAllVersionsOfBookName,
  setBooksPairTexts,
  setSentences,
  toggleIsSelectingBookId,
  toggleIsSelectingUploadVersion,
  toggleIsQuickViewBooksPair,
  toggleIsFetching,
} from "../../redux/select-reducer";
import SelectTexts from "./SelectTexts";
import Preloader from "../common/preloader/Preloader";
class SelectTextsContainerAPI extends React.Component {
  constructor(props) {
    super(props);
  }

  componentDidMount() {
    this.fetchAllBookIdsWithNames();
  }

  failureCallback = () => {
    console.log(this.props.maxUploadedVersion);
  };

  fetchAllBookIdsWithNames = () => {
    //debugger;
    this.props.toggleIsFetching(true);
    let where = "bookSentenceId";
    let whereValue = 1;
    let orderBy = "bookId";
    let startUpVersion = 1;
    //api/BookTexts/BooksIds/?where=bookSentenceId&whereValue=1&orderBy=bookId&needPostSelect=true&postWhere=UploadVersion&postWhereValue=1
    //api/BookTexts/BooksIds/?where=bookSentenceId&whereValue=1&orderBy=bookId
    //return Axios.get(`api/BookTexts/BooksIds/?where=${where}&whereValue=${whereValue}&orderBy=${orderBy}`)
    //FromDbWhere/?where="bookSentenceId"&whereValue=1
    return Axios.get(`api/BookTexts/BooksNamesIds/?where=${where}&whereValue=${whereValue}&startUploadVersion=${startUpVersion}`)
      .then((Response) => {
        this.props.toggleIsFetching(false);
        console.log(Response);
        console.log("axios: sending this to props:", Response.data.bookNamesVersion1SortedByIds);
        //debugger;
        this.props.setAllBookIdsWithNames(Response.data.bookNamesVersion1SortedByIds);
        console.log("axios: finished sending to props");
        let s = Response.data.sortedBooksIdsLength;
        //debugger;
        return s;
      })
      .catch(this.failureCallback);
  };

  fetchAllVersionsOfSelectedBook = (bookId) => {
    //debugger;
    this.props.toggleIsFetching(true);
    let where = "bookSentenceId";
    let whereValue = 1;
    //api/BookTexts/BookNameVersions/?where="bookId"&whereValue=1
    return Axios.get(`api/BookTexts/BookNameVersions/?where=${where}&whereValue=${whereValue}&bookId=${bookId}`)
      .then((Response) => {
        this.props.toggleIsFetching(false);
        console.log("Response of BookNameVersions", Response);
        this.props.setAllVersionsOfBookName(Response.data.selectedBookIdAllVersions);
        console.log("axios: finished sending to props");
        return Response;
      })
      .catch(this.failureCallback);
  };

  fetchChosenVersionOfSelectedBooksPair = (selectedBookId, selectedVersion) => {
    //debugger;
    this.props.toggleIsFetching(true);
    let where1 = "bookId";
    let where1Value = selectedBookId;
    let where2 = "uploadVersion";
    let where2Value = selectedVersion;
    return Axios.get(`api/BookTexts/BooksPairTexts/?where1=${where1}&where1Value=${where1Value}&where2=${where2}&where2Value=${where2Value}`)
      .then((Response) => {
        this.props.toggleIsFetching(false);
        console.log("Response of BooksPairTexts", Response);
        this.props.setAllVersionsOfBookName(Response.data.selectedBooksPairTexts);
        console.log("axios: finished sending to props");
        return Response;
      })
      .catch(this.failureCallback);
  };

  //Let to switch on BooksNames choosing (return to the previous)
  switchBooksIdsOn = () => {
    this.props.toggleIsSelectingUploadVersion(false, "");
    this.fetchAllBookIdsWithNames().then((r) => {
      this.props.toggleIsSelectingBookId(true);
    });
    return 0;
  };

  //Let to switch on BookVersions choosing
  switchBookVersionsOn = (bookId) => {
    this.props.toggleIsSelectingBookId(false);
    this.fetchAllVersionsOfSelectedBook(bookId).then((r) => {
      this.props.toggleIsSelectingUploadVersion(true);
    });
    return { bookId };
  };

  //Let to switch on QuickView
  switchQuickViewOn = (selectedBookId, selectedVersion) => {
    this.props.toggleIsSelectingUploadVersion(false);
    this.fetchChosenVersionOfSelectedBooksPair(selectedBookId, selectedVersion).then((r) => {
      this.props.toggleIsQuickViewBooksPair(true);
      return { selectedVersion };
    });
  };

  //Let to switch on NEXT choosing
  nextAfterQuickView = (i) => {
    this.props.toggleIsSelectingUploadVersion(false);
    this.props.toggleIsQuickViewBooksPair(false);
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

// toggleIsSelectingBookId={this.props.toggleIsSelectingBookId}
// toggleIsSelectingUploadVersion={this.props.toggleIsSelectingUploadVersion}

let SelectTextsContainer = connect(mapStateToProps, {
  setAllBookIdsWithNames,
  setAllVersionsOfBookName,
  setBooksPairTexts,
  setSentences,
  toggleIsSelectingBookId,
  toggleIsSelectingUploadVersion,
  toggleIsQuickViewBooksPair,
  toggleIsFetching,
})(SelectTextsContainerAPI);

export default SelectTextsContainer;
