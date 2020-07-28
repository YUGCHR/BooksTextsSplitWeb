import React from "react";
import Axios from "axios";
import ReactScrollWheelHandler from "react-scroll-wheel-handler";
import { connect } from "react-redux";
import { setAllBookIdsWithNames, setAllVersionsOfBookName, setSentences, toggleIsSelectingBookId, toggleIsSelectingUploadVersion, toggleIsFetching } from "../../redux/select-reducer";
import SelectTexts from "./SelectTexts";
import Preloader from "../common/preloader/Preloader";
class SelectTextsContainerAPI extends React.Component {
  constructor(props) {
    super(props);
  }

  componentDidMount() {
    this.fetchAllBookIdsWithNames();
    this.fetchAllVersionsOfSelectedBook();
  }

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
        debugger;
        this.props.setAllBookIdsWithNames(Response.data.bookNamesVersion1SortedByIds);
        console.log("axios: finished sending to props");
        let s = Response.data.sortedBooksIdsLength;
        //debugger;
        return s;
      })
      .catch(this.failureCallback);
  };

  failureCallback = () => {
    console.log(this.props.maxUploadedVersion);
  };

  fetchAllVersionsOfSelectedBook = () => {
    //debugger;
    this.props.toggleIsFetching(true);
    let where = "bookId";
    let whereValue = 1;    

    //api/BookTexts/BookNameVersions/?where="bookId"&whereValue=1
    return Axios.get(`api/BookTexts/BookNameVersions/?where=${where}&whereValue=${whereValue}`)
      .then((Response) => {
        this.props.toggleIsFetching(false);
        console.log("Response of BookNameVersions", Response);        
        //debugger;
        this.props.setAllVersionsOfBookName(Response.data.allVersionsOfBooksNames);        
        console.log("axios: finished sending to props");        
        return Response;
      })
      .catch(this.failureCallback);
  };
  /* fetchSentences = (languageId) => {
        this.props.toggleIsFetching(true);
        Axios
            .get(`/api/BookTexts/count/${languageId}`)
            .then(Response => {                
                this.props.toggleIsFetching(false);
                this.props.setSentencesCount(Response.data.sentencesCount, languageId);            
                this.props.sentencesCount[languageId] === 0
                    ? this.props.toggleIsLoading(false, languageId)
                    : this.props.toggleIsLoading(true, languageId);
                
            });
        this.props.toggleIsFetching(true);
        debugger;
        Axios        
            .get(`/api/BookTexts/BookText/${languageId}`)
            .then(Response => {
                this.props.toggleIsFetching(false);
                this.props.setSentences(Response.data.sentences, languageId);
            });
    }
 */
  render() {
    //console.log('container render starts', this.props.allBookNamesSortedByIds.length, this.props.allBookNamesSortedByIds);

    return (
      <>
        {this.props.isFetching ? <Preloader /> : null}
        <SelectTexts
          sentencesCount={this.props.sentencesCount}
          engSentences={this.props.engSentences}
          rusSentences={this.props.rusSentences}
          scrollLineUp={this.props.scrollLineUp}
          scrollLineDown={this.props.scrollLineDown}
          allBookNamesSortedByIds={this.props.allBookNamesSortedByIds}          
          allVersionsOfBooksNames={this.props.allVersionsOfBooksNames}
          bookNamesVersion1SortedByIds={this.props.bookNamesVersion1SortedByIds}
          isSelectingBookId={this.props.isSelectingBookId}
          isSelectingUploadVersion={this.props.isSelectingUploadVersion}
          isFetching={this.props.isFetching}
          fetchAllBookIdsWithNames={this.fetchAllBookIdsWithNames}
          fetchAllVersionsOfSelectedBook={this.fetchAllVersionsOfSelectedBook}
          toggleIsSelectingBookId={this.props.toggleIsSelectingBookId}
          toggleIsSelectingUploadVersion={this.props.toggleIsSelectingUploadVersion}
        />
      </>
    );
  }
}

let mapStateToProps = (state) => {
  return {
    lastSentenceNumber: state.selectTextsPage.lastSentenceNumber,
    readingSentenceNumber: state.selectTextsPage.readingSentenceNumber,
    sentencesOnPageTop: state.selectTextsPage.sentencesOnPageTop,
    sentencesOnPageBottom: state.selectTextsPage.sentencesOnPageBottom,
    sentencesCount: state.selectTextsPage.sentencesCount,
    isSelectingBookId: state.selectTextsPage.isSelectingBookId,
    isSelectingUploadVersion: state.selectTextsPage.isSelectingUploadVersion,  
    isFetching: state.selectTextsPage.isFetching,
    allBookNamesSortedByIds: state.selectTextsPage.allBookNamesSortedByIds,
    bookNamesVersion1SortedByIds: state.selectTextsPage.bookNamesVersion1SortedByIds,
    allVersionsOfBooksNames: state.selectTextsPage.allVersionsOfBooksNames
  };
};

let SelectTextsContainer = connect(mapStateToProps, { setAllBookIdsWithNames, setAllVersionsOfBookName, setSentences, toggleIsSelectingBookId, toggleIsSelectingUploadVersion, toggleIsFetching })(SelectTextsContainerAPI);

export default SelectTextsContainer;
