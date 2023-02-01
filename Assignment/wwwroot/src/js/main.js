const form = document.getElementById("detailsForm");
const dob = form.elements['dob'];
const age = form.elements['age'];
const self_pay = form.elements['selfPay'];
const fee_schedule = form.elements['feeSchedule'];
const upload_img = form.elements['avatar-img-input'];
const avatar = document.getElementById("avatar");
const avatar_img_remove = document.querySelector('.avatar-img-remove');

self_pay.addEventListener('change', () => {
    if (self_pay.checked == true) {
        fee_schedule.disabled = false;
    } else {
        fee_schedule.disabled = true;
    }
})

upload_img.addEventListener('change', () => {
    avatar.src = URL.createObjectURL(upload_img.files[0]);
    avatar_img_remove.style.display = "block";
})

avatar_img_remove.addEventListener('click', () => {
    upload_img.value = '';
    avatar.src = './src/images/avatar_none.png';
    avatar_img_remove.style.display = "none";
})

dob.addEventListener('change', (e) => {
    function getAge(date) {
        const dob = new Date(date);
        now = new Date();
        cYear = now.getFullYear();
        // cMonth = now.getMonth();
        // cDay = now.getDate();
        bYear = dob.getFullYear();
        // bMonth = dob.getMonth();
        // bDay = dob.getDate();
        dYear = cYear - bYear;
        // dMonth = cMonth - bMonth;
        // dDay = cDay - bDay;
        // return { dYear, dMonth, dDay };
        return { dYear };
    }
    _age = getAge(dob.value);
    age.value = _age.dYear + " Y";
})

params = new URLSearchParams(window.location.search)
id = params.get("id")
if (id != null) {
    fetch(`https://localhost:7139/api/patientdemographics/${id}`)
        .then(res => res.json())
        .then(data => {

            // If patient found then display patient data
            // Else show alert message that patient not found
            if (data != -1) {
                document.getElementsByName("fname")[0].value = data.patientList[0].firstName;
                document.getElementsByName("chart-no")[0].textContent = data.patientList[0].chartNo;
                document.getElementsByName("lname")[0].value = data.patientList[0].lastName;
                document.getElementsByName("mname")[0].value = data.patientList[0].middleName;
                document.getElementsByName("sex")[0].value = data.patientList[0].sexTypeId;
                if (data.patientList[0].dob != null) {
                    document.getElementsByName("dob")[0].value = data.patientList[0].dob.split("T")[0];
                    dob.dispatchEvent(new Event('change'));
                }
            } else {
                alert("Patient not found! Error:404");
                location.replace(location.href.split("?")[0]);
            }
        })
}

form.addEventListener('submit', (event) => {
    event.preventDefault()

    const phoneNumber = form.elements['phoneNumber']
    const sex = form.elements['sex']
    const requiredFields = document.getElementsByClassName('required-field');

    flag = true;

    for (i = 0; i < requiredFields.length; i++) {
        // console.log(requiredFields.item(i).value);
        if (requiredFields.item(i).value == "") {
            flag = false;
            requiredFields.item(i).nextElementSibling.style.display = "block";
        } else {
            requiredFields.item(i).nextElementSibling.style.display = "none";
        }
    }

    if (flag) {
        if (_age.dYear < 18 && phoneNumber.value == "") {
            if (sex.value == "Male") {
                ageWiseText = "he"
            } else if (sex.value == "Female") {
                ageWiseText = "she"
            } else {
                ageWiseText = "he/she"
            }
            alert("Please add a contact for the patient as " + ageWiseText + " is minor")
        } else {

            if (id != null) {
                fetch(`https://localhost:7139/api/patientdemographics/${id}`, {
                    method: 'PUT',
                    headers: {
                        "content-type": "application/json"
                    },
                    body: JSON.stringify({
                        "firstName": document.getElementsByName("fname")[0].value,
                        "middleName": document.getElementsByName("mname")[0].value,
                        "lastName": document.getElementsByName("lname")[0].value,
                        "dob": document.getElementsByName("dob")[0].value,
                        "sexTypeId": parseInt(document.getElementsByName("sex")[0].value)
                    })
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data) {
                            console.log("Updated Successfully!");
                            showToastMessage();
                        }
                        else
                        console.log("Error!");
                    })
                } else {
                    fetch(`https://localhost:7139/api/patientdemographics/`, {
                        method: 'POST',
                        headers: {
                            "content-type": "application/json"
                        },
                        body: JSON.stringify({
                            "firstName": document.getElementsByName("fname")[0].value,
                            "middleName": document.getElementsByName("mname")[0].value,
                            "lastName": document.getElementsByName("lname")[0].value,
                            "dob": document.getElementsByName("dob")[0].value,
                            "sexTypeId": parseInt(document.getElementsByName("sex")[0].value)
                        })
                    })
                    .then(res => res.json())
                    .then(data => {
                        if (data) {
                            console.log("Inserted Successfully!\nId: " + data);
                            setTimeout(() => {
                                location.replace(location + "?id=" + data)
                            }, 3000);
                            showToastMessage();
                        }
                        else
                            console.log("Error!");
                    })
            }

            const formData = new FormData(form);
            cbs = document.querySelectorAll('input[type=checkbox]')
            cbs.forEach(cb => {
                formData.append(cb.name, cb.checked);
            });
            const entries = formData.entries();
            const details = Object.fromEntries(entries);

            console.log(details);
        }
    }
})

form.addEventListener('reset', () => {
    location.reload();
})

contactFieldCounter = 1;
function addContactField() {
    contactHTML = `<div id="single-contact-${++contactFieldCounter}" class="single-contact w-100">
    <fieldset>
        <legend>
                <select class="contact-type" name="contact-type">
                    <option value="Home">Home</option>
                    <option value="Work">Work</option>
                    <option value="Other">Other</option>
                </select>
        </legend>
        <div class="address">
            <div class="flex">
                <div class="text-bolder" style="width: 98%;">
                    Address <i
                        class="fa-solid fa-circle-plus addAddressFieldBtn"
                        onclick="addAddressField(this)"
                        style="display: none;"></i>
                </div>
                <div style="width: 2%;">
                    <i class="fa-solid fa-trash-can def-color"
                        onclick="removeContactField(this)"></i>
                </div>
            </div>
            <div class="address-section">
                <div class="single-address">
                    <div>
                        Street
                        <div class="flex" style="margin-right: 50%;">
                            <input type="text" name="street">
                        </div>
                    </div>
                    <div class="flex" style="column-gap: 2%;">
                        <div style="width: 24%;">
                            Zip
                            <input type="text" name="zip">
                        </div>
                        <div style="width: 24%;">
                            City
                            <input type="text" name="city">
                        </div>
                        <div style="width: 24%;">
                            State
                            <input type="text" name="state">
                        </div>
                        <div style="width: 24%;">
                            Country
                            <select name="country">
                                <option value="IN">IN</option>
                                <option value="US">US</option>
                            </select>
                        </div>
                        <div style="width: 4%;">
                            <i class="fa-solid fa-trash-can def-color"
                                onclick="removeAddressField(this)"></i>
                        </div>
                    </div>
                </div>

            </div>
        </div>
        <div>
            <div class="phone-main-section">
                <div class="text-bolder">
                    Phone <i class="fa-solid fa-circle-plus def-color"
                        onclick="addPhoneField(this)"></i>
                </div>
                <div class="phone-section">
                    <div class="phone-label-section flex"
                        style="column-gap: 2%; margin-bottom: 0.5rem; border-bottom: solid 1px rgba(212,215,220,.5098039215686274);">
                        <div style="width: 20%;">
                            Type
                        </div>
                        <div style="width: 20%;">
                            Code
                        </div>
                        <div style="width: 20%;">
                            Number
                        </div>
                        <div style="width: 20%;">
                            Ext.
                        </div>
                    </div>
                    <div class="phone-input-section flex"
                        style="column-gap: 2%;">
                        <div style="width: 20%;">
                            <select name="phonetype">
                                <option value="Cell">Cell</option>
                                <option value="Landline">Landline</option>
                            </select>
                        </div>
                        <div style="width: 20%;">
                            <select name="countrtycode">
                                <option value="+91">+91 India</option>
                                <option value="+1">+1 United States</option>
                            </select>
                        </div>
                        <div style="width: 20%;">
                            <input type="text" name="phoneNumber"
                                placeholder="Number" value="1">
                        </div>
                        <div style="width: 20%;">
                            <input type="text" name="Ext"
                                placeholder="Ext." value=""
                                style="display: none;">
                        </div>
                        <i class="fa-solid fa-trash-can def-color"
                            onclick="removePhoneField(this)"></i>
                    </div>
                </div>
            </div>

            <div class="fax-main-section">
                <div class="text-bolder">
                    Fax <i class="fa-solid fa-circle-plus def-color"
                        onclick="addFaxField(this)"></i>
                </div>
                <div class="fax-section">
                    <div class="fax-label-section flex"
                        style="column-gap: 2%; margin-bottom: 0.5rem; border-bottom: solid 1px rgba(212,215,220,.5098039215686274);">
                        <div style="width: 20%;">
                            Code
                        </div>
                        <div style="width: 20%;">
                            Number
                        </div>
                    </div>
                    <div class="fax-input-section flex" style="column-gap: 2%;">
                        <div style="width: 20%;">
                            <input type="text" placeholder="Code"
                                name="faxcode">
                        </div>
                        <div style="width: 20%;">
                            <input type="text" placeholder="Number"
                                name="faxnumber">
                        </div>
                        <div style="width: 2%;">
                            <i class="fa-solid fa-trash-can def-color"
                            onclick="removeFaxField(this)"></i>
                        </div>
                    </div>
                </div>
            </div>

            <div class="email-section">
                <div class="text-bolder">
                    Email <i class="fa-solid fa-circle-plus def-color"
                        onclick="addEmailField(this)"></i>
                </div>
                <div class="multi-email">
                    <div class="single-email flex" style="column-gap: 2%;">
                        <div style="width: 50%;">
                            <input type="email" name="email">
                        </div>
                        <div style="width: 2%;">
                            <i class="fa-solid fa-trash-can def-color"
                                onclick="removeEmailField(this)"></i>
                        </div>
                    </div>
                </div>
            </div>
            <div class="website-section">
                <div class="text-bolder">
                    Website <i class="fa-solid fa-circle-plus def-color"
                        onclick="addWebsiteField(this)"></i>
                </div>
                <div class="multi-website">

                <div class="single-website flex"
                style="column-gap: 2%;">
                <div style="width: 50%;">
                    <input class="website-input" type="url"
                        name="website" onchange="isValidUrl(this)">
                </div>
                <div class="flex" style="width: 2%; gap: 20%;">
                    <i target="_blank" class="fa-solid fa-earth-americas" onclick="openURL(this);" style="display: none;"></i>
                    <i class="fa-solid fa-trash-can"
                        onclick="removeWebsiteField(this)"></i>
                </div>
            </div>
                </div>
            </div>
        </div>
    </fieldset>
    </div>`
    document.querySelector(".multi-contact").insertAdjacentHTML("beforeend", contactHTML);
}

function removeContactField(btn) {
    btn.closest('.single-contact').remove();
}

function addAddressField(btn) {
    addressHTML = `<div class="single-address">
    <div>
        Street
        <div class="flex" style="margin-right: 50%;">
            <input type="text" name="street">
        </div>
    </div>
    <div class="flex" style="column-gap: 2%;">
        <div style="width: 24%;">
            Zip
            <input type="text" name="zip">
        </div>
        <div style="width: 24%;">
            City
            <input type="text" name="city">
        </div>
        <div style="width: 24%;">
            State
            <input type="text" name="state">
        </div>
        <div style="width: 24%;">
            Country
            <select name="country">
                <option value="IN">IN</option>
                <option value="US">US</option>
            </select>
        </div>
        <div style="width: 4%;">
            <i class="fa-solid fa-trash-can def-color"
                onclick="removeAddressField(this)"></i>
        </div>
    </div>
</div>`
    btn.closest('.address').querySelector('.address-section').insertAdjacentHTML("beforeend", addressHTML);
    btn.style.display = "none";
}

function removeAddressField(btn) {
    btn.closest('.address').querySelector('.addAddressFieldBtn').style.display = "";
    btn.closest('.single-address').remove();
}

function addPhoneField(btn) {
    phoneInputSectionHTML = `<div class="phone-input-section flex"
    style="column-gap: 2%;">
    <div style="width: 20%;">
        <select name="phonetype">
            <option value="Cell">Cell</option>
            <option value="Landline">Landline</option>
        </select>
    </div>
    <div style="width: 20%;">
        <select name="countrtycode">
            <option value="+91">+91 India</option>
            <option value="+1">+1 United States</option>
        </select>
    </div>
    <div style="width: 20%;">
        <input type="text" name="phoneNumber"
            placeholder="Number" value="">
    </div>
    <div style="width: 20%;">
        <input type="text" name="phoneNumber"
            placeholder="Ext." value=""
            style="display: none;">
    </div>
    <i class="fa-solid fa-trash-can def-color"
        onclick="removePhoneField(this)"></i>
</div>`;

    phoneLabelSectionHTML = `<div class="phone-label-section flex" style="column-gap: 2%; margin-bottom: 0.5rem; border-bottom: solid 1px rgba(212,215,220,.5098039215686274);">
    <div style="width: 20%;">
        Type
    </div>
    <div style="width: 20%;">
        Code
    </div>
    <div style="width: 20%;">
        Number
    </div>
    <div style="width: 20%;">
        Ext.
    </div>
</div>`

    if (btn.closest('.phone-main-section').querySelector('.phone-section').childElementCount == 0) {
        btn.closest('.phone-main-section').querySelector('.phone-section').insertAdjacentHTML("beforeend", phoneLabelSectionHTML);
    }
    btn.closest('.phone-main-section').querySelector('.phone-section').insertAdjacentHTML("beforeend", phoneInputSectionHTML);
}

function removePhoneField(btn) {
    if (btn.closest('.phone-section').childElementCount == 2) {
        btn.closest('.phone-section').querySelector('.phone-label-section').remove();
    }
    btn.closest('.phone-input-section').remove();
}

function addFaxField(btn) {
    faxInputSectionHTML = `<div class="fax-input-section flex" style="column-gap: 2%;">
    <div style="width: 20%;">
        <input type="text" placeholder="Code"
            name="faxcode">
    </div>
    <div style="width: 20%;">
        <input type="text" placeholder="Number"
            name="faxnumber">
    </div>
    <div style="width: 2%;">
        <i class="fa-solid fa-trash-can def-color"
        onclick="removeFaxField(this)"></i>
    </div>`;

    faxLabelSectionHTML = `<div class="fax-label-section flex"
    style="column-gap: 2%; margin-bottom: 0.5rem; border-bottom: solid 1px rgba(212,215,220,.5098039215686274);">
    <div style="width: 20%;">
        Code
    </div>
    <div style="width: 20%;">
        Number
    </div>
</div>`

    if (btn.closest('.fax-main-section').querySelector('.fax-section').childElementCount == 0) {
        btn.closest('.fax-main-section').querySelector('.fax-section').insertAdjacentHTML("beforeend", faxLabelSectionHTML);
    }
    btn.closest('.fax-main-section').querySelector('.fax-section').insertAdjacentHTML("beforeend", faxInputSectionHTML);
}

function removeFaxField(btn) {
    if (btn.closest('.fax-section').childElementCount == 2) {
        btn.closest('.fax-section').querySelector('.fax-label-section').remove();
    }
    btn.closest('.fax-input-section').remove();
}

function addEmailField(btn) {
    emailHTML = `<div class="single-email flex" name="single-email"
    style="column-gap: 2%;">
    <div style="width: 50%;">
        <input type="email" name="email">
    </div>
    <div style="width: 2%;">
        <i class="fa-solid fa-trash-can def-color"
            onclick="removeEmailField(this)"></i>
    </div>
</div>`
    btn.closest('.email-section').querySelector('.multi-email').insertAdjacentHTML("beforeend", emailHTML);
}

function removeEmailField(btn) {
    btn.closest('.single-email').remove();
}

function addWebsiteField(btn) {
    websiteHTML = `<div class="single-website flex"
    style="column-gap: 2%;">
    <div style="width: 50%;">
        <input class="website-input" type="url"
            name="website" onchange="isValidUrl(this)">
    </div>
    <div class="flex" style="width: 2%; gap: 20%;">
        <i target="_blank" class="fa-solid fa-earth-americas" onclick="openURL(this);" style="display: none;"></i>
        <i class="fa-solid fa-trash-can"
            onclick="removeWebsiteField(this)"></i>
    </div>
</div>`
    btn.closest('.website-section').querySelector('.multi-website').insertAdjacentHTML("beforeend", websiteHTML);
}

function removeWebsiteField(btn) {
    btn.closest('.single-website').remove();
}

function openSidebar() {
    document.querySelector(".sidebar").style.width = "13%";
    document.getElementById("closeSidebar").style.display = "";
    document.getElementById("openSidebar").style.display = "none";
}

function closeSidebar() {
    document.querySelector(".sidebar").style.width = "0.7%";
    document.getElementById("closeSidebar").style.display = "none";
    document.getElementById("openSidebar").style.display = "";
}

function openOtherDetails() {
    document.querySelector(".other-detail-content").style.display = "";
    document.getElementById("closeOtherDetails").style.display = "";
    document.getElementById("openOtherDetails").style.display = "none";
}

function closeOtherDetails() {
    document.querySelector(".other-detail-content").style.display = "none";
    document.getElementById("closeOtherDetails").style.display = "none";
    document.getElementById("openOtherDetails").style.display = "";
}

var showToastMessageTimeout;
function showToastMessage() {
    let ele = document.querySelector('.toast-message');

    ele.classList.add("toast-message-show");
    showToastMessageTimeout = setTimeout(() => {
        ele.classList.remove("toast-message-show");
    }, 3000);
}

function removeToastMessage() {
    clearTimeout(showToastMessageTimeout);
    document.querySelector('.toast-message').classList.remove('toast-message-show');
}

const details_div = document.querySelector('.details')
const innernav = document.querySelector('.innernav')

details_div.addEventListener("scroll", (event) => {
    let scroll = details_div.scrollTop;
    if (scroll == 0) {
        innernav.style.boxShadow = "none";
    } else {
        innernav.style.boxShadow = "0px 4px 5px rgb(162 165 169 / 25%)";
    }
});

function isValidUrl(ele) {
    const pattern = new RegExp(
        '^([a-zA-Z]+:\\/\\/)?' + // protocol
        '((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|' + // domain name
        '((\\d{1,3}\\.){3}\\d{1,3}))' + // OR IP (v4) address
        '(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*' + // port and path
        '(\\?[;&a-z\\d%_.~+=-]*)?' + // query string
        '(\\#[-a-z\\d_]*)?$', // fragment locator
        'i'
    );
    if (pattern.test(ele.value)) {
        ele.closest('.single-website').querySelector('.fa-earth-americas').style.display = '';
        ele.style.border = "";
    } else {
        ele.closest('.single-website').querySelector('.fa-earth-americas').style.display = 'none';
        if (ele.value != "") {
            ele.style.border = "2px solid red";
        } else {
            ele.style.border = "";
        }
    }
}

function openURL(ele) {
    url = ele.closest('.single-website').querySelector('.website-input').value;
    window.open(url, '_blank');
}